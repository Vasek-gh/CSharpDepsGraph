using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System.Globalization;
using CSharpDepsGraph.Building.Entities;
using System.Data;
using System.Diagnostics;
using CSharpDepsGraph.Building.Services;

namespace CSharpDepsGraph.Building;

/// <summary>
/// <see cref="IGraph"/> builder
/// </summary>
public sealed class GraphBuilder
{
    private readonly ILogger _logger;
    private readonly IFilter _filter;
    private readonly Metrics _metrics;
    private readonly BuildingData _graphData;
    private readonly CultureInfo _cultureInfo;
    private readonly ILoggerFactory _loggerFactory;
    private readonly SymbolComparer _symbolComparer;
    private readonly GeneratedCodeDetector _generatedCodeDetector;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphBuilder"/> class.
    /// </summary>
    public GraphBuilder(
        ILoggerFactory loggerFactory,
        GraphBuildOptions options,
        CultureInfo? cultureInfo = null
        )
    {
        _loggerFactory = loggerFactory;
        _cultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;

        _logger = CreateLogger();
        _metrics = new();
        _symbolComparer = new(options);
        _generatedCodeDetector = new(options);

        _filter = new Filter(options, _symbolComparer);

        _graphData = new(
            _metrics,
            _symbolComparer,
            SymbolUidGenerator.Create(options)
            );
    }

    /// <summary>
    /// Build code graph
    /// </summary>
    public Task<IGraph> Run(IEnumerable<Project> projects, CancellationToken cancellationToken)
    {
        return DoWithMeasurement<IGraph>(_logger, async () =>
        {
            var projectsVariants = GetProjectsVariants(projects);
            foreach (var projectVariants in projectsVariants)
            {
                await HandleProjectVariants(projectVariants, cancellationToken);
            }

            BuildLinks();

            return new Graph()
            {
                Root = _graphData.Root,
                Links = _graphData.Links
            };
        });
    }

    private void BuildLinks()
    {
        var logger = CreateLogger(nameof(LinkBuilder));
        var _ = DoWithMeasurement(logger, () =>
        {
            new LinkBuilder(logger, _graphData, _generatedCodeDetector).Run();
            return Task.CompletedTask;
        });
    }

    private Task HandleProjectVariants(Project[] projectVariants, CancellationToken cancellationToken)
    {
        if (projectVariants.Length == 0)
        {
            return Task.CompletedTask;
        }

        if (projectVariants.Length == 1)
        {
            return HandleProject(projectVariants[0], cancellationToken);
        }

        var assemblyName = projectVariants[0].AssemblyName;
        return DoWithMeasurement(CreateLogger(assemblyName), async () =>
        {
            foreach (var project in projectVariants)
            {
                await HandleProject(project, cancellationToken);
            }
        });
    }

    private Task HandleProject(Project project, CancellationToken cancellationToken)
    {
        if (!project.SupportsCompilation)
        {
            return Task.CompletedTask;
        }

        var logger = CreateLogger(project.Name);
        return DoWithMeasurement(logger, async () =>
        {
            await _generatedCodeDetector.PrepareProjectAsync(project, cancellationToken);

            var projectPath = project.FilePath ?? $"{project.Name}.dll";

            var compilation = await GetCompilation(project, logger, cancellationToken);
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var generatedFileKind = _generatedCodeDetector.GetGeneratedFileKindAsync(syntaxTree, cancellationToken);
                if (generatedFileKind == GeneratedFileKind.Hiden)
                {
                    continue;
                }

                HandleSyntax(
                    logger,
                    syntaxTree,
                    compilation,
                    generatedFileKind != GeneratedFileKind.None,
                    projectPath,
                    cancellationToken
                );
            }
        });
    }

    private void HandleSyntax(
        ILogger logger,
        SyntaxTree syntaxTree,
        Compilation compilation,
        bool isGenerated,
        string projectPath,
        CancellationToken cancellationToken
        )
    {
        var syntaxRoot = syntaxTree.GetRoot(cancellationToken);
        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        var syntaxVisitor = new SyntaxVisitor(
            logger,
            _filter,
            isGenerated,
            projectPath,
            _graphData,
            semanticModel
            );

        syntaxVisitor.Visit(syntaxRoot);
    }

    private async Task<Compilation> GetCompilation(Project project, ILogger logger, CancellationToken cancellationToken)
    {
        var compilation = await project.GetCompilationAsync(cancellationToken)
            ?? throw new Exception($"Fail to get compilation for project {project.Name}");

        HandleErrors(logger, project, compilation);

        return compilation;
    }

    private async Task<T> DoWithMeasurement<T>(ILogger logger, Func<Task<T>> action)
    {
        logger.LogInformation("Begin handle...");

        if (!_logger.IsEnabled(LogLevel.Debug))
        {
            return await action();
        }

        _metrics.BeginScope(logger);

        var sw = new Stopwatch();
        sw.Start();

        var totalMemoryStart = GC.GetTotalMemory(false);

        var result = await action();

        var totalMemoryEnd = GC.GetTotalMemory(false);

        sw.Stop();

        _metrics.ElapsedTime.Set(sw.Elapsed);
        _metrics.AllocatedMemory.Set(totalMemoryEnd - totalMemoryStart);
        _metrics.EndScope();

        return result;
    }

    private async Task DoWithMeasurement(ILogger logger, Func<Task> action)
    {
        logger.LogInformation("Begin handle...");

        if (!_logger.IsEnabled(LogLevel.Debug))
        {
            await action();
            return;
        }

        _metrics.BeginScope(logger);

        var sw = new Stopwatch();
        sw.Start();

        var totalMemoryStart = GC.GetTotalMemory(false);

        await action();

        var totalMemoryEnd = GC.GetTotalMemory(false);

        sw.Stop();

        _metrics.ElapsedTime.Set(sw.Elapsed);
        _metrics.AllocatedMemory.Set(totalMemoryEnd - totalMemoryStart);
        _metrics.EndScope();
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