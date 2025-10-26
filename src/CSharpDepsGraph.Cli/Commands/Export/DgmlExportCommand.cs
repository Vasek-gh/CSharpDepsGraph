using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Cli.Commands.Settings;
using CSharpDepsGraph.Export.Dgml;

namespace CSharpDepsGraph.Cli.Commands.Export;

internal sealed class DgmlExportCommand : IGraphCommand
{
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ExportSettings _settings;

    public DgmlExportCommand(ILoggerFactory loggerFactory, ExportSettings settings)
    {
        _logger = loggerFactory.CreateLogger(nameof(DgmlExportCommand));
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

            _logger.LogDebug("Mutation...");

            var graph = Utils.GetHierarchyExportMutator(_settings).Run(ctx.Graph);

            _logger.LogDebug("Export...");

            using var stream = Utils.CreateOutputStream(ctx.InputFile, _settings.OutputPath, "dgml");

            await new DgmlExport(_loggerFactory.CreateLogger<DgmlExport>()).Run(graph, stream, cancellationToken);
        });
    }
}