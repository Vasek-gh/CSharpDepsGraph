using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Cli.Commands;
using CSharpDepsGraph.Cli.Options;
using System.CommandLine;

namespace CSharpDepsGraph.Cli.CommandLine;

internal sealed class JsonExportCliCommand : BaseCliCommand
{
    private readonly ICommandFactory _commandFactory;
    private readonly OptionsHost<JsonExportOptions> _optionsHost;

    public JsonExportCliCommand(ICommandFactory commandFactory)
        : base("json", "Json export")
    {
        _commandFactory = commandFactory;

        _optionsHost = new OptionsHost<JsonExportOptions>(this)
            .AddOption(ExportOptionsFactory.OutputFileName, (o, v) => o.OutputPath = v?.FullName)
            .AddOption(ExportOptionsFactory.HideExternal, (o, v) => o.HideExternal = v)
            .AddOption(ExportOptionsFactory.ExportLevelFull, (o, v) => o.ExportLevel = v)
            .AddOption(ExportOptionsFactory.NodeFilters, (o, v) => o.NodeFilters = v ?? [])
            .AddOption(ExportOptionsFactory.Json.Format, (o, v) => o.Format = v);
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
        return _commandFactory.CreateJsonExport(loggerFactory, buildOptions, _optionsHost.GetValue(parseResult));
    }
}