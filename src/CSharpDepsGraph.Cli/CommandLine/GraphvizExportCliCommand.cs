using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Cli.Commands;
using CSharpDepsGraph.Cli.Options;
using System.CommandLine;

namespace CSharpDepsGraph.Cli.CommandLine;

internal sealed class GraphvizExportCliCommand : BaseCliCommand
{
    private readonly ICommandFactory _commandFactory;
    private readonly OptionsHost<ExportOptions> _optionsHost;

    public GraphvizExportCliCommand(ICommandFactory commandFactory)
        : base("graphviz", "Graphviz export")
    {
        _commandFactory = commandFactory;

        _optionsHost = new OptionsHost<ExportOptions>(this)
            .AddOption(ExportOptionsFactory.OutputFileName, (o, v) => o.OutputFileName = v?.FullName)
            .AddOption(ExportOptionsFactory.HideExternal, (o, v) => o.HideExternal = v)
            .AddOption(ExportOptionsFactory.ExportLevelOneLevel, (o, v) => o.ExportLevel = v)
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
        return _commandFactory.CreateGraphVizExport(loggerFactory, buildOptions, _optionsHost.GetValue(parseResult));
    }
}
