using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Export.Json;

namespace CSharpDepsGraph.Cli.Commands.Export;

public sealed class JsonExportCommand : IHandlerCommand
{
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Options.JsonExportOptions _settings;

    public JsonExportCommand(ILoggerFactory loggerFactory, Options.JsonExportOptions settings)
    {
        _logger = loggerFactory.CreateLogger(nameof(JsonExportCommand));
        _loggerFactory = loggerFactory;
        _settings = settings;
    }

    public Task Execute(GraphContext graphContext, CancellationToken cancellationToken)
    {
        return CommandsUtils.ExecuteWithReport(_logger, async () =>
        {
            _logger.LogDebug("Mutation...");

            var graph = CommandsUtils.GetHierarchyExportTransformer(_settings).Execute(graphContext.Graph);

            _logger.LogDebug("Export...");

            using var stream = CommandsUtils.CreateOutputStream(graphContext.InputFile, _settings.OutputPath, "json");

            var options = new CSharpDepsGraph.Export.Json.JsonExportOptions() // todo create cli options
            {
                FormatOutput = _settings.Format,
                BasePath = Path.GetDirectoryName(graphContext.InputFile)
            };

            await new JsonExport(_loggerFactory.CreateLogger<JsonExport>(), options).RunAsync(graph, stream, cancellationToken);
        });
    }
}