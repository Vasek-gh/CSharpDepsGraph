using CSharpDepsGraph.Cli.Commands;
using CSharpDepsGraph.Cli.Commands.Export;
using CSharpDepsGraph.Export.Json;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace CSharpDepsGraph.Cli.CommandLine;

internal static class JsonExportFactory
{
    public static Command CreateCliCommand(ICommandFactory commandFactory)
    {
        return new CommandBuilder<Options.JsonExportOptions>()
            .WithName("json")
            .WithDescription("Json export")
            .WithFactory((lf, bo, o) => commandFactory.CreateJsonExport(lf, bo, o))
            .AddOption(CommandOptions.OutputFileName, (o, v) => o.OutputFileName = v?.FullName)
            .AddOption(CommandOptions.HideExternal, (o, v) => o.HideExternal = v)
            .AddOption(CommandOptions.ExportLevelFull, (o, v) => o.ExportLevel = v)
            .AddOption(CommandOptions.NodeFilters, (o, v) => o.NodeFilters = v ?? [])
            .AddOption(CommandOptions.Json.Format, (o, v) => o.Format = v)
            .AddOption(CommandOptions.Json.ExcludeLocations, (o, v) => o.ExcludeLocations = v)
            .AddOption(CommandOptions.Json.InlinePaths, (o, v) => o.InlinePaths = v)
            .Build();
    }

    public static IGraphCommand CreateGraphCommand(
        ILoggerFactory loggerFactory,
        Options.JsonExportOptions cliOptions,
        string basePath
        )
    {
        var options = new CSharpDepsGraph.Export.Json.JsonExportOptions()
        {
            FormatOutput = cliOptions.Format,
            ExcludeLocations = cliOptions.ExcludeLocations,
            InlinePaths = cliOptions.InlinePaths,
            BasePath = basePath
        };

        var transformer = CommandsUtils.GetHierarchyExportTransformer(cliOptions);
        var jsonExport = new JsonExport(loggerFactory.CreateLogger<JsonExport>(), options);

        return new JsonExportCommand(
            loggerFactory.CreateLogger<JsonExportCommand>(),
            transformer,
            jsonExport,
            cliOptions
            );
    }
}