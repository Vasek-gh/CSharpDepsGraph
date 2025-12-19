using System.CommandLine.Invocation;
using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Cli.Commands;
using CSharpDepsGraph.Cli.Options;

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
            .AddOption(ExportOptionsFactory.OutputFileName, (o, v) => o.OutputPath = v?.FullName)
            .AddOption(ExportOptionsFactory.HideExternal, (o, v) => o.HideExternal = v)
            .AddOption(ExportOptionsFactory.ExportLevelOneLevel, (o, v) => o.ExportLevel = v)
            .AddOption(ExportOptionsFactory.SymbolFilters, (o, v) => o.SymbolFilters = v ?? []);
    }

    protected override void BeforeExecute(ILogger logger, InvocationContext ctx)
    {
        logger.Verbose(_optionsHost.GetValue(ctx));
    }

    protected override ICommand CreateCommand(
        InvocationContext ctx,
        ILoggerFactory loggerFactory,
        BuildOptions buildOptions
        )
    {
        return _commandFactory.CreateGraphVizExport(loggerFactory, buildOptions, _optionsHost.GetValue(ctx));
    }
}
