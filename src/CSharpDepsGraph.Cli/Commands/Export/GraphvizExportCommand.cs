using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Export.Graphviz;
using CSharpDepsGraph.Cli.Options;
using CSharpDepsGraph.Transforming;

namespace CSharpDepsGraph.Cli.Commands.Export;

public sealed class GraphvizExportCommand : IGraphCommand
{
    private readonly ILogger _logger;
    private readonly ITransformer _transformer;
    private readonly GraphvizExport _graphvizExport;
    private readonly ExportOptions _options;

    public GraphvizExportCommand(
        ILogger<GraphvizExportCommand> logger,
        ITransformer transformer,
        GraphvizExport graphvizExport,
        ExportOptions options
        )
    {
        _logger = logger;
        _transformer = transformer;
        _graphvizExport = graphvizExport;
        _options = options;
    }

    public Task Execute(GraphContext graphContext, CancellationToken cancellationToken)
    {
        return CommandsUtils.ExecuteWithReport(_logger, async () =>
        {
            _logger.LogDebug("Mutation...");

            var graph = _transformer.Execute(graphContext.Graph);

            _logger.LogDebug("Export...");

            using var stream = CommandsUtils.CreateOutputStream(graphContext.InputFile, _options.OutputFileName, "dot");

            await _graphvizExport.RunAsync(graph, stream, cancellationToken);
        });
    }
}