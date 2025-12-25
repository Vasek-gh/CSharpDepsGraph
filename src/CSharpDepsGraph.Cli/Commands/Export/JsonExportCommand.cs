using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Export.Json;

namespace CSharpDepsGraph.Cli.Commands.Export;

public sealed class JsonExportCommand : IHandlerCommand
{
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Options.JsonExportOptions _options;

    public JsonExportCommand(ILoggerFactory loggerFactory, Options.JsonExportOptions options)
    {
        _logger = loggerFactory.CreateLogger(nameof(JsonExportCommand));
        _loggerFactory = loggerFactory;
        _options = options;
    }

    public Task Execute(GraphContext graphContext, CancellationToken cancellationToken)
    {
        return CommandsUtils.ExecuteWithReport(_logger, async () =>
        {
            _logger.LogDebug("Mutation...");

            var graph = CommandsUtils.GetHierarchyExportTransformer(_options).Execute(graphContext.Graph);

            _logger.LogDebug("Export...");

            using var stream = CommandsUtils.CreateOutputStream(graphContext.InputFile, _options.OutputFileName, "json");

            var options = new CSharpDepsGraph.Export.Json.JsonExportOptions()
            {
                FormatOutput = _options.Format,
                ExcludeLocations = _options.ExcludeLocations,
                InlinePaths = _options.InlinePaths,
                BasePath = Path.GetDirectoryName(graphContext.InputFile)
            };

            await new JsonExport(_loggerFactory.CreateLogger<JsonExport>(), options)
                .RunAsync(graph, stream, cancellationToken);
        });
    }
}