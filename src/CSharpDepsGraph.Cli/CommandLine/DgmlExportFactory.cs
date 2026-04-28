using CSharpDepsGraph.Cli.Commands;
using CSharpDepsGraph.Cli.Commands.Export;
using CSharpDepsGraph.Export.Dgml;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace CSharpDepsGraph.Cli.CommandLine;

internal static class DgmlExportFactory
{
    public static Command CreateCliCommand(ICommandFactory commandFactory)
    {
        return new CommandBuilder<Options.ExportOptions>()
            .WithName("dgml")
            .WithDescription("Dgml export")
            .WithFactory((lf, bo, o) => commandFactory.CreateDgmlExport(lf, bo, o))
            .AddOption(CommandOptions.OutputFileName, (o, v) => o.OutputFileName = v?.FullName)
            .AddOption(CommandOptions.HideExternal, (o, v) => o.HideExternal = v)
            .AddOption(CommandOptions.ExportLevelFull, (o, v) => o.ExportLevel = v)
            .AddOption(CommandOptions.NodeFilters, (o, v) => o.NodeFilters = v ?? [])
            .Build();
    }

    public static IGraphCommand CreateGraphCommand(ILoggerFactory loggerFactory, Options.ExportOptions exportOptions)
    {
        var transformer = CommandsUtils.GetHierarchyExportTransformer(exportOptions);
        var dgmlExport = new DgmlExport(loggerFactory.CreateLogger<DgmlExport>());

        return new DgmlExportCommand(
            loggerFactory.CreateLogger<DgmlExportCommand>(),
            transformer,
            dgmlExport,
            exportOptions
            );
    }
}