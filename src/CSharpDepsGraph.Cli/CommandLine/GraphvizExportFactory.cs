using CSharpDepsGraph.Cli.Commands;
using CSharpDepsGraph.Cli.Commands.Export;
using CSharpDepsGraph.Export.Graphviz;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace CSharpDepsGraph.Cli.CommandLine;

internal static class GraphvizExportFactory
{
    public static Command CreateCliCommand(ICommandFactory commandFactory)
    {
        return new CommandBuilder<Options.ExportOptions>()
            .WithName("graphviz")
            .WithDescription("Graphviz export")
            .WithFactory((lf, bo, o) => commandFactory.CreateGraphVizExport(lf, bo, o))
            .AddOption(CommandOptions.OutputFileName, (o, v) => o.OutputFileName = v?.FullName)
            .AddOption(CommandOptions.HideExternal, (o, v) => o.HideExternal = v)
            .AddOption(CommandOptions.ExportLevelOneLevel, (o, v) => o.ExportLevel = v)
            .AddOption(CommandOptions.NodeFilters, (o, v) => o.NodeFilters = v ?? [])
            .Build();
    }

    public static IGraphCommand CreateGraphCommand(ILoggerFactory loggerFactory, Options.ExportOptions exportOptions)
    {
        var transformer = CommandsUtils.GetFlatExportTransformer(exportOptions);
        var graphvizExport = new GraphvizExport(loggerFactory.CreateLogger<GraphvizExport>());

        return new GraphvizExportCommand(
            loggerFactory.CreateLogger<GraphvizExportCommand>(),
            transformer,
            graphvizExport,
            exportOptions
            );
    }
}
