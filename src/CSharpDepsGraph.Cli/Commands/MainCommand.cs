using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Building;
using CSharpDepsGraph.Cli.Commands.Settings;

namespace CSharpDepsGraph.Cli.Commands;

internal sealed class MainCommand
{
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IGraphCommand _command;
    private readonly BuildSettings _settings;

    public MainCommand(ILoggerFactory loggerFactory, BuildSettings settings, IGraphCommand command)
    {
        _settings = ValidateSettings(settings);

        _logger = loggerFactory.CreateLogger(nameof(MainCommand));
        _loggerFactory = loggerFactory;

        _command = command;
    }

    public Task Execute(CancellationToken cancellationToken)
    {
        return Utils.ExecuteWithReport(_logger, async () =>
        {
            _logger.LogValue(_settings.FileName);
            _logger.LogValue(_settings.Configuration);
            _logger.LogValue(_settings.Properties);

            var graphContext = await BuildGraph(cancellationToken);

            await _command.Execute(graphContext, cancellationToken);
        });
    }

    private async Task<GraphContext> BuildGraph(CancellationToken cancellationToken)
    {
        MSBuildLocator.RegisterDefaults();

        var props = CreateProps();

        using var workspace = MSBuildWorkspace.Create(props);
        //workspace.LoadMetadataForReferencedProjects = true;

        var projects = Path.GetExtension(_settings.FileName) switch
        {
            ".sln" => await OpenSolution(workspace, cancellationToken),
            ".csproj" => await OpenProject(workspace, cancellationToken),
            _ => throw new Exception($"Unsupported extension: {Path.GetExtension(_settings.FileName)}")
        };

        var graph = await new GraphBuilder(_loggerFactory).Run(projects, cancellationToken);

        return new GraphContext()
        {
            Graph = graph,
            InputFile = _settings.FileName,
            InputProjects = projects
        };
    }

    private async Task<IEnumerable<Project>> OpenSolution(
        MSBuildWorkspace workspace,
        CancellationToken cancellationToken
        )
    {
        _logger.LogInformation("Open solution...");

        var progress = new Progress(_logger);
        var solution = await workspace.OpenSolutionAsync(
            solutionFilePath: _settings.FileName,
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
        _logger.LogDebug("Open project...");

        var project = await workspace.OpenProjectAsync(
            _settings.FileName,
            cancellationToken: cancellationToken
            );

        return new[] { project };
    }

    private Dictionary<string, string> CreateProps()
    {
        var result = new Dictionary<string, string>();
        if (_settings.Configuration != null)
        {
            result.Add("Configuration", _settings.Configuration);
        }

        foreach (var prop in _settings.Properties ?? [])
        {
            result.Add(prop.Key, prop.Value);
        }

        //result.Add("DisableBuild", "true");

        return result;
    }

    private static BuildSettings ValidateSettings(BuildSettings settings)
    {
        var fileNameError = Utils.GetFileNameError(settings.FileName);
        if (fileNameError != null)
        {
            throw new Exception(fileNameError);
        }

        foreach (var prop in settings.Properties)
        {
            if (string.IsNullOrWhiteSpace(prop.Key))
            {
                throw new ArgumentException("Property must have name");
            }

            if (string.IsNullOrWhiteSpace(prop.Value))
            {
                throw new ArgumentException("Property must have value");
            }
        }

        return new BuildSettings()
        {
            FileName = settings.FileName,
            Configuration = string.IsNullOrWhiteSpace(settings.Configuration) ? null : settings.Configuration,
            Properties = settings.Properties
        };
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