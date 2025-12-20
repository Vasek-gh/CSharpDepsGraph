using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Building;
using CSharpDepsGraph.Cli.Options;

namespace CSharpDepsGraph.Cli.Commands;

public sealed class BuildCommand : ICommand
{
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IHandlerCommand _command;
    private readonly BuildOptions _options;

    public BuildCommand(
        ILoggerFactory loggerFactory,
        BuildOptions options,
        IHandlerCommand command
        )
    {
        _loggerFactory = loggerFactory;
        _options = options.Validate();
        _command = command;

        _logger = loggerFactory.CreateLogger(nameof(BuildCommand));
    }

    public Task Execute(CancellationToken cancellationToken)
    {
        return CommandsUtils.ExecuteWithReport(_logger, () =>
        {
            return DoExecute(cancellationToken);
        });
    }

    private async Task DoExecute(CancellationToken cancellationToken)
    {
        MSBuildLocator.RegisterDefaults();

        using var workspace = CreateWorkspace();

        var projects = await OpenProjects(workspace, cancellationToken);
        var graph = await CreateGraph(projects, cancellationToken);

        var ctx = new GraphContext()
        {
            Graph = graph,
            InputFile = _options.FileName,
        };

        await _command.Execute(ctx, cancellationToken);
    }

    private MSBuildWorkspace CreateWorkspace()
    {
        var props = CreateProps();
        var workspace = MSBuildWorkspace.Create(props);
        workspace.LoadMetadataForReferencedProjects = false;

        return workspace;
    }

    private Task<IGraph> CreateGraph(
        IEnumerable<Project> projects,
        CancellationToken cancellationToken
        )
    {
        var builder = new GraphBuilder(_loggerFactory, _options.GraphOptions);
        return builder.Run(projects, cancellationToken);
    }

    private async Task<IEnumerable<Project>> OpenProjects(
        MSBuildWorkspace workspace,
        CancellationToken cancellationToken
        )
    {
        var filePath = _options.FileName;
        var fileExtension = Path.GetExtension(filePath);
        var projects = fileExtension switch
        {
            ".sln" => await OpenSolution(workspace, cancellationToken),
            ".csproj" => await OpenProject(workspace, cancellationToken),
            _ => throw new Exception($"Unsupported extension: {fileExtension}")
        };

        return projects;
    }

    private async Task<IEnumerable<Project>> OpenSolution(
        MSBuildWorkspace workspace,
        CancellationToken cancellationToken
        )
    {
        _logger.LogInformation("Open solution...");

        var progress = new Progress(_logger);
        var solution = await workspace.OpenSolutionAsync(
            solutionFilePath: _options.FileName,
            progress: progress,
            cancellationToken: cancellationToken
            );

        return solution.Projects;
    }

    private async Task<IEnumerable<Microsoft.CodeAnalysis.Project>> OpenProject(
        MSBuildWorkspace workspace,
        CancellationToken cancellationToken
        )
    {
        _logger.LogInformation("Open project...");

        var project = await workspace.OpenProjectAsync(
            _options.FileName,
            cancellationToken: cancellationToken
            );

        return new[] { project };
    }

    private Dictionary<string, string> CreateProps()
    {
        var result = new Dictionary<string, string>();
        if (_options.Configuration != null)
        {
            result.Add("Configuration", _options.Configuration);
        }

        foreach (var prop in _options.Properties ?? [])
        {
            result.Add(prop.Key, prop.Value);
        }

        result.Add("CustomBeforeMicrosoftCommonTargets", "D:/Src/inject.props"); // todo

        result.Add("Configuration", "Debug");
        result.Add("Platform", "Any CPU");

        result.Add("GenerateDocumentationFile", "false");
        result.Add("CopyLocalLockFileAssemblies", "false");

        result.Add("SkipCompilerExecution", "true");
        result.Add("ProvideCommandLineArgs", "true");

        result.Add("DesignTimeBuild", "true");
        result.Add("RunAnalyzers", "false");
        result.Add("RunAnalyzersDuringBuild", "false");

        return result;
    }

    private class Progress : IProgress<ProjectLoadProgress>
    {
        private readonly ILogger _logger;
        public Dictionary<string, List<ProjectLoadProgress>> LoadedProjects = new();

        public Progress(ILogger logger)
        {
            _logger = logger;
        }

        public void Report(ProjectLoadProgress value)
        {
            //_logger.LogTrace($"{value.Operation}: {value.FilePath}");
        }
    }
}