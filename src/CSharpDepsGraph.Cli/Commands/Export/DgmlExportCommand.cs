using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Export.Dgml;
using CSharpDepsGraph.Cli.Options;

namespace CSharpDepsGraph.Cli.Commands.Export;

internal sealed class DgmlExportCommand : IHandlerCommand
{
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ExportOptions _options;

    public DgmlExportCommand(ILoggerFactory loggerFactory, ExportOptions options)
    {
        _logger = loggerFactory.CreateLogger(nameof(DgmlExportCommand));
        _loggerFactory = loggerFactory;

        _options = options;
    }

    public Task Execute(GraphContext ctx, CancellationToken cancellationToken)
    {
        return CommandsUtils.ExecuteWithReport(_logger, async () =>
        {
            _logger.LogDebug("Mutation...");

            var graph = CommandsUtils.GetHierarchyExportMutator(_options).Run(ctx.Graph);

            _logger.LogDebug("Export...");

            using var stream = CommandsUtils.CreateOutputStream(ctx.InputFile, _options.OutputPath, "dgml");

            await new DgmlExport(_loggerFactory.CreateLogger<DgmlExport>()).Run(graph, stream, cancellationToken);
        });
    }
}