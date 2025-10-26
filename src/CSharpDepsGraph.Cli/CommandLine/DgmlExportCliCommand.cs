using System.CommandLine.Invocation;
using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Cli.Commands;
using CSharpDepsGraph.Cli.Commands.Export;
using CSharpDepsGraph.Cli.Commands.Settings;

namespace CSharpDepsGraph.Cli.CommandLine;

internal class DgmlExportCliCommand : BaseCliCommand
{
    private readonly OptionsHost<ExportSettings> _optionsHost;

    public DgmlExportCliCommand()
        : base("dgml", "Dgml export")
    {
        _optionsHost = new OptionsHost<ExportSettings>(this, (host) =>
        {
            return new ExportSettings
            {
                OutputPath = host.GetOption(() => ExportOptions.OutputFileName)?.FullName,
                HideExternal = host.GetOption(() => ExportOptions.HideExternal),
                ExportLevel = host.GetOption(() => ExportOptions.ExportLevelFull),
                SymbolFilters = host.GetOption(() => ExportOptions.SymbolFilters) ?? []
            };
        });
    }

    protected override IGraphCommand CreateCommand(InvocationContext ctx, ILoggerFactory loggerFactory)
    {
        var settings = _optionsHost.GetSettings(ctx);

        return new DgmlExportCommand(loggerFactory, settings);
    }
}