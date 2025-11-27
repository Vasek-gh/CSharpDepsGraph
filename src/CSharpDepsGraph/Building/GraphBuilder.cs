using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System.Globalization;
using CSharpDepsGraph.Building.Entities;
using CSharpDepsGraph.Building.Generators;
using CSharpDepsGraph.Building.Services;
using System.Data;

namespace CSharpDepsGraph.Building;

/// <summary>
/// <see cref="IGraph"/> builder
/// </summary>
public sealed class GraphBuilder
{
    private readonly ILogger _logger;
    private readonly Counters _counters;
    private readonly GraphData _graphData;
    private readonly CultureInfo _cultureInfo;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ISymbolIdGenerator _idGenerator;
    private readonly SymbolComparer _symbolComparer;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphBuilder"/> class.
    /// </summary>
    public GraphBuilder(
        ILoggerFactory loggerFactory,
        CultureInfo? cultureInfo = null
        )
    {
        _loggerFactory = loggerFactory;
        _cultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;

        _logger = CreateLogger();
        _counters = new Counters();
        _idGenerator = new SymbolIdGenerator(loggerFactory, false);
        _symbolComparer = new(false, false, null);
        _graphData = new(_counters, _symbolComparer, _idGenerator);
    }

    /// <summary>
    /// Build code graph
    /// </summary>
    public async Task<IGraph> Run(IEnumerable<Project> projects, CancellationToken cancellationToken)
    {
        var projectsVariants = GetProjectsVariants(projects);
        foreach (var projectVariants in projectsVariants)
        {
            await HandleProjectVariants(projectVariants, cancellationToken);
        }

        var m1 = GC.GetTotalMemory(false);
        new LinkBuilder(CreateLogger(nameof(LinkBuilder)), _graphData).Run();
        var m2 = GC.GetTotalMemory(false) - m1;

        _counters.Report(_logger); // todo kill
        _idGenerator.WriteStatistic();

        return new Graph()
        {
            Root = _graphData.Root,
            Links = _graphData.Links
        };
    }

    private async Task HandleProjectVariants(Project[] projectVariants, CancellationToken cancellationToken)
    {
        var firstProject = projectVariants[0];

        _logger.LogInformation($"Begin handle {firstProject.AssemblyName} variants");

        foreach (var project in projectVariants)
        {
            await HandleProject(project, cancellationToken);
        }

        _logger.LogDebug($"{firstProject.AssemblyName} variants handled");
    }

    private async Task HandleProject(Project project, CancellationToken cancellationToken)
    {
        var logger = CreateLogger(project.Name);

        logger.LogInformation($"Begin handle project...");

        var compilation = await GetCompilation(project, logger, cancellationToken);
        var generatedFiles = await GetGeneratedFiles(project, cancellationToken);

        var projectPath = project.FilePath ?? $"{project.Name}.dll";

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            HandleSyntax(syntaxTree, compilation, generatedFiles, projectPath, cancellationToken);
        }

        logger.LogDebug($"Project handled");
    }

    private void HandleSyntax(
        SyntaxTree syntaxTree,
        Compilation compilation,
        ISet<string> generatedFiles,
        string projectPath,
        CancellationToken cancellationToken
        )
    {
        var fileIsFromSourceGenerators = generatedFiles.Contains(syntaxTree.FilePath);
        if (!fileIsFromSourceGenerators
            && GeneratedCodeUtilities.IsGeneratedCode(syntaxTree, cancellationToken))
        {
            return;
        }

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var syntaxVisitor = new SyntaxVisitor(
            CreateLogger(nameof(SyntaxVisitor)),
            _graphData,
            semanticModel,
            fileIsFromSourceGenerators,
            projectPath
            );

        var syntaxRoot = syntaxTree.GetRoot(cancellationToken);

        syntaxVisitor.Visit(syntaxRoot);
    }

    private async Task<Compilation> GetCompilation(Project project, ILogger logger, CancellationToken cancellationToken)
    {
        // todo check if project can GetCompilationAsync
        var compilation = await project.GetCompilationAsync(cancellationToken)
            ?? throw new Exception($"Fail to get compilation for project {project.Name}");

        HandleErrors(logger, project, compilation);

        return compilation;
    }

    private static async Task<ISet<string>> GetGeneratedFiles(Project project, CancellationToken cancellationToken)
    {
        return (await project.GetSourceGeneratedDocumentsAsync(cancellationToken))
            .Where(doc => doc.FilePath != null)
            .Select(doc => doc.FilePath ?? "")
            .ToHashSet();
    }

    private void HandleErrors(ILogger logger, Project project, Compilation compilation)
    {
        var diagErrors = compilation.GetDiagnostics()
            .Where(m => m.Severity == DiagnosticSeverity.Error)
            .ToArray();

        if (diagErrors.Length == 0)
        {
            return;
        }

        foreach (var error in diagErrors)
        {
            logger.LogError(GetDiagMessage(error));
        }

        var fatalMessage = $"Project {Path.GetFileName(project.FilePath)} has errors, build break";
        logger.LogCritical(fatalMessage);

        throw new CSharpDepsGraphException(fatalMessage);
    }

    private string GetDiagMessage(Diagnostic diagnostic)
    {
        var span = diagnostic.Location.GetMappedLineSpan();
        var line = span.StartLinePosition.Line + 1;
        var column = span.StartLinePosition.Character + 1;
        var path = span.Path;

        return $"{path}:{line}:{column} {diagnostic.GetMessage(_cultureInfo)}";
    }

    private ILogger CreateLogger(string? category = null)
    {
        return Utils.CreateLogger<GraphBuilder>(_loggerFactory, category);
    }

    private static Project[][] GetProjectsVariants(IEnumerable<Project> projects)
    {
        return projects.ToLookup(p => p.AssemblyName)
            .Select(i => i.ToArray())
            .Where(i => i.Length > 0)
            .ToArray();
    }
}