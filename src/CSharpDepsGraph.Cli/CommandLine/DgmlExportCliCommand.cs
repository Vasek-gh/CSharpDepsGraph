using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Cli.Commands;
using CSharpDepsGraph.Cli.Options;
using System.CommandLine;

namespace CSharpDepsGraph.Cli.CommandLine;

internal sealed class DgmlExportCliCommand : BaseCliCommand
{
    private readonly ICommandFactory _commandFactory;
    private readonly OptionsHost<Options.ExportOptions> _optionsHost;

    public DgmlExportCliCommand(ICommandFactory commandFactory)
        : base("dgml", "Dgml export")
    {
        _commandFactory = commandFactory;

        _optionsHost = new OptionsHost<ExportOptions>(this)
            .AddOption(ExportOptionsFactory.OutputFileName, (o, v) => o.OutputPath = v?.FullName)
            .AddOption(ExportOptionsFactory.HideExternal, (o, v) => o.HideExternal = v)
            .AddOption(ExportOptionsFactory.ExportLevelFull, (o, v) => o.ExportLevel = v)
            .AddOption(ExportOptionsFactory.NodeFilters, (o, v) => o.NodeFilters = v ?? []);
    }

    protected override void BeforeExecute(ILogger logger, ParseResult parseResult)
    {
        logger.Verbose(_optionsHost.GetValue(parseResult));
    }

    protected override ICommand CreateCommand(
        ParseResult parseResult,
        ILoggerFactory loggerFactory,
        BuildOptions buildOptions
        )
    {
        return _commandFactory.CreateDgmlExport(loggerFactory, buildOptions, _optionsHost.GetValue(parseResult));
    }
}