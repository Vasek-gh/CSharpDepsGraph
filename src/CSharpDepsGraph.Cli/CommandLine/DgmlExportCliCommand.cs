using System.CommandLine.Invocation;
using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Cli.Commands;
using CSharpDepsGraph.Cli.Commands.Export;
using CSharpDepsGraph.Cli.Options;

namespace CSharpDepsGraph.Cli.CommandLine;

internal sealed class DgmlExportCliCommand : BaseCliCommand
{
    private readonly OptionsHost<Options.ExportOptions> _optionsHost;

    public DgmlExportCliCommand()
        : base("dgml", "Dgml export")
    {
        _optionsHost = AddOptionHost<ExportOptions>((logger, o) => logger.Verbose(o))
            .AddOption(ExportOptionsFactory.OutputFileName, (o, v) => o.OutputPath = v?.FullName)
            .AddOption(ExportOptionsFactory.HideExternal, (o, v) => o.HideExternal = v)
            .AddOption(ExportOptionsFactory.ExportLevelFull, (o, v) => o.ExportLevel = v)
            .AddOption(ExportOptionsFactory.SymbolFilters, (o, v) => o.SymbolFilters = v ?? []);
    }

    protected override IGraphHandlerCommand CreateHandlerCommand(InvocationContext ctx, ILoggerFactory loggerFactory)
    {
        var settings = _optionsHost.GetValue(ctx);

        return new DgmlExportCommand(loggerFactory, settings);
    }
}