using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Export.Dgml;
using CSharpDepsGraph.Cli.Options;
using CSharpDepsGraph.Transforming;

namespace CSharpDepsGraph.Cli.Commands.Export;

public sealed class DgmlExportCommand : IGraphCommand
{
    private readonly ILogger _logger;
    private readonly ITransformer _transformer;
    private readonly DgmlExport _dgmlExport;
    private readonly ExportOptions _options;

    public DgmlExportCommand(
        ILogger<DgmlExportCommand> logger,
        ITransformer transformer,
        DgmlExport dgmlExport,
        ExportOptions options
        )
    {
        _logger = logger;
        _transformer = transformer;
        _dgmlExport = dgmlExport;
        _options = options;
    }

    public Task Execute(GraphContext graphContext, CancellationToken cancellationToken)
    {
        return CommandsUtils.ExecuteWithReport(_logger, async () =>
        {
            _logger.LogDebug("Mutation...");

            var graph = _transformer.Execute(graphContext.Graph);

            _logger.LogDebug("Export...");

            using var stream = CommandsUtils.CreateOutputStream(graphContext.InputFile, _options.OutputFileName, "dgml");

            await _dgmlExport.RunAsync(graph, stream, cancellationToken);
        });
    }
}