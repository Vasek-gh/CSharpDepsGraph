using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Export.Json;
using CSharpDepsGraph.Transforming;

namespace CSharpDepsGraph.Cli.Commands.Export;

public sealed class JsonExportCommand : IGraphCommand
{
    private readonly ILogger _logger;
    private readonly ITransformer _transformer;
    private readonly JsonExport _jsonExport;
    private readonly Options.JsonExportOptions _options;

    public JsonExportCommand(
        ILogger<JsonExportCommand> logger,
        ITransformer transformer,
        JsonExport jsonExport,
        Options.JsonExportOptions options
        )
    {
        _logger = logger;
        _transformer = transformer;
        _jsonExport = jsonExport;
        _options = options;
    }

    public Task Execute(GraphContext graphContext, CancellationToken cancellationToken)
    {
        return CommandsUtils.ExecuteWithReport(_logger, async () =>
        {
            _logger.LogDebug("Mutation...");

            var graph = _transformer.Execute(graphContext.Graph);

            _logger.LogDebug("Export...");

            using var stream = CommandsUtils.CreateOutputStream(graphContext.InputFile, _options.OutputFileName, "json");

            await _jsonExport.RunAsync(graph, stream, cancellationToken);
        });
    }
}