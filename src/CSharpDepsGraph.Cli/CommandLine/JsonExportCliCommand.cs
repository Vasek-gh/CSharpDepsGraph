using System.CommandLine.Invocation;
using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Cli.Commands;
using CSharpDepsGraph.Cli.Commands.Export;
using CSharpDepsGraph.Cli.Commands.Settings;

namespace CSharpDepsGraph.Cli.CommandLine;

internal class JsonExportCliCommand : BaseCliCommand
{
    private readonly OptionsHost<JsonExportSettings> _optionsHost;

    public JsonExportCliCommand()
        : base("json", "Json export")
    {
        _optionsHost = new OptionsHost<JsonExportSettings>(this, (host) =>
        {
            return new JsonExportSettings
            {
                OutputPath = host.GetOption(() => ExportOptions.OutputFileName)?.FullName,
                HideExternal = host.GetOption(() => ExportOptions.HideExternal),
                ExportLevel = host.GetOption(() => ExportOptions.ExportLevelFull),
                SymbolFilters = host.GetOption(() => ExportOptions.SymbolFilters) ?? [],
                Format = host.GetOption(() => ExportOptions.Json.Format)
            };
        });
    }

    protected override IGraphCommand CreateCommand(InvocationContext ctx, ILoggerFactory loggerFactory)
    {
        var settings = _optionsHost.GetSettings(ctx);

        return new JsonExportCommand(loggerFactory, settings);
    }
}