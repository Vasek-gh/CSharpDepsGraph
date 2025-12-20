using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Export.Graphviz;
using CSharpDepsGraph.Cli.Options;

namespace CSharpDepsGraph.Cli.Commands.Export;

internal sealed class GraphvizExportCommand : IHandlerCommand
{
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ExportOptions _options;

    public GraphvizExportCommand(ILoggerFactory loggerFactory, ExportOptions options)
    {
        _logger = loggerFactory.CreateLogger(nameof(GraphvizExportCommand));
        _loggerFactory = loggerFactory;

        _options = options;
    }

    public Task Execute(GraphContext ctx, CancellationToken cancellationToken)
    {
        return CommandsUtils.ExecuteWithReport(_logger, async () =>
        {
            _logger.LogDebug("Mutation...");

            var graph = CommandsUtils.GetFlatExportMutator(_options).Execute(ctx.Graph);

            _logger.LogDebug("Export...");

            using var stream = CommandsUtils.CreateOutputStream(ctx.InputFile, _options.OutputPath, "dot");

            await new GraphvizExport(_loggerFactory.CreateLogger<GraphvizExport>()).Run(graph, stream, cancellationToken);
        });
    }
}