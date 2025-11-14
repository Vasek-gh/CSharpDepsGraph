using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Globalization;
using CSharpDepsGraph.Building.Entities;
using CSharpDepsGraph.Building.Generators;

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
    private readonly ISymbolIdGenerator _symbolIdBuilder;
    private readonly LinkedSymbolsMap _linkedSymbolsMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphBuilder"/> class.
    /// </summary>
    public GraphBuilder(
        ILoggerFactory loggerFactory,
        CultureInfo? cultureInfo = null,
        ISymbolIdGenerator? symbolIdBuilder = null
        )
    {
        _logger = loggerFactory.CreateLogger<GraphBuilder>();
        _loggerFactory = loggerFactory;
        _cultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;

        _counters = new Counters();
        _graphData = new GraphData(_counters);
        _linkedSymbolsMap = new(_counters);
        _symbolIdBuilder = symbolIdBuilder ?? new SymbolIdGenerator(loggerFactory, false, false);
    }

    /// <summary>
    /// Build code graph
    /// </summary>
    public async Task<IGraph> Run(IEnumerable<Project> projects, CancellationToken cancellationToken)
    {
        foreach (var project in projects)
        {
            await CreateProjectNodes(project, cancellationToken);
        }

        new NodeLinkBuilder(_loggerFactory.CreateLogger<NodeLinkBuilder>(), _counters, _symbolIdBuilder, _graphData).Run();

        _counters.Report(_logger); // todo kill
        _symbolIdBuilder.WriteStatistic();

        return new Graph()
        {
            Root = _graphData.Root,
            Links = _graphData.Links
        };
    }

    private async Task CreateProjectNodes(Project project, CancellationToken cancellationToken)
    {
        var projectPath = project.FilePath
            ?? $"{project.Name}.dll";

        var logger = Utils.CreateLogger<GraphBuilder>(_loggerFactory, project.Name);

        logger.LogInformation($"Begin handle project...");

        var compilation = await GetCompilation(project, logger, cancellationToken);
        var generatedFiles = await GetGeneratedFiles(project, cancellationToken);

        _linkedSymbolsMap.Clear();
        var linkedSymbolsMap = _linkedSymbolsMap;

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            HandleSyntax(syntaxTree, compilation, linkedSymbolsMap, generatedFiles, cancellationToken);
        }

        var symbolVisitor = new SymbolVisitor(
            Utils.CreateLogger<SymbolVisitor>(_loggerFactory, compilation.Assembly.Name),
            projectPath,
            generatedFiles,
            _symbolIdBuilder,
            linkedSymbolsMap,
            _graphData
        );

        symbolVisitor.Visit(compilation.Assembly);
    }

    private void HandleSyntax(
        SyntaxTree syntaxTree,
        Compilation compilation,
        LinkedSymbolsMap linkedSymbolsMap,
        ISet<string> generatedFiles,
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
            Utils.CreateLogger<SymbolVisitor>(_loggerFactory, syntaxTree.FilePath),
            semanticModel,
            _symbolIdBuilder,
            linkedSymbolsMap,
            fileIsFromSourceGenerators
            );

        syntaxVisitor.Visit(syntaxTree.GetRoot(cancellationToken));
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
}