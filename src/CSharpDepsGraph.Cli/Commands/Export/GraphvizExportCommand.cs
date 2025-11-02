using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Cli.Commands.Settings;
using CSharpDepsGraph.Export.Graphviz;

namespace CSharpDepsGraph.Cli.Commands.Export;

internal sealed class GraphvizExportCommand : IGraphCommand
{
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ExportSettings _settings;

    public GraphvizExportCommand(ILoggerFactory loggerFactory, ExportSettings settings)
    {
        _logger = loggerFactory.CreateLogger(nameof(GraphvizExportCommand));
        _loggerFactory = loggerFactory;

        _settings = settings;
    }

    public async Task Execute(GraphContext ctx, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Execute");
        _logger.LogValue(_settings.OutputPath);
        _logger.LogValue(_settings.HideExternal);
        _logger.LogValue(_settings.ExportLevel);
        _logger.LogValue(_settings.SymbolFilters);

        _logger.LogDebug("Mutation...");

        var graph = Utils.GetFlatExportMutator(_settings).Run(ctx.Graph);

        _logger.LogDebug("Export...");

        using var stream = Utils.CreateOutputStream(ctx.InputFile, _settings.OutputPath, "dot");

        await new GraphvizExport(_loggerFactory.CreateLogger<GraphvizExport>()).Run(graph, stream, cancellationToken);
    }
}