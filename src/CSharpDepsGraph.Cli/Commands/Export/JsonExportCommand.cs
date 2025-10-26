using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Cli.Commands.Settings;
using CSharpDepsGraph.Export.Json;

namespace CSharpDepsGraph.Cli.Commands.Export;

internal sealed class JsonExportCommand : IGraphCommand
{
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly JsonExportSettings _settings;

    public JsonExportCommand(ILoggerFactory loggerFactory, JsonExportSettings settings)
    {
        _logger = loggerFactory.CreateLogger(nameof(JsonExportCommand));
        _loggerFactory = loggerFactory;
        _settings = settings;
    }

    public Task Execute(GraphContext ctx, CancellationToken cancellationToken)
    {
        return Utils.ExecuteWithReport(_logger, async () =>
        {
            _logger.LogValue(_settings.OutputPath);
            _logger.LogValue(_settings.HideExternal);
            _logger.LogValue(_settings.ExportLevel);
            _logger.LogValue(_settings.SymbolFilters);
            _logger.LogValue(_settings.Format);

            _logger.LogDebug("Mutation...");

            var graph = Utils.GetHierarchyExportMutator(_settings).Run(ctx.Graph);

            _logger.LogDebug("Export...");

            using var stream = Utils.CreateOutputStream(ctx.InputFile, _settings.OutputPath, "json");

            await new JsonExport(_loggerFactory.CreateLogger<JsonExport>(), _settings.Format).Run(graph, stream, cancellationToken);
        });
    }
}