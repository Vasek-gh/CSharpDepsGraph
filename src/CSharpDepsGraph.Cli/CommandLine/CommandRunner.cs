using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Cli.Commands;
using CSharpDepsGraph.Cli.Options;
using System.CommandLine;
using Microsoft.Extensions.Logging.Console;
using System.Text;
using System.CommandLine.Parsing;

namespace CSharpDepsGraph.Cli.CommandLine;

internal class CommandRunner<TOptions>
    where TOptions : class, IOptions, new()
{
    private readonly Command _command;
    private readonly OptionsHost<LoggingOptions> _loggingOptionsHost;
    private readonly OptionsHost<BuildingOptions> _buildingOptionsHost;
    private readonly OptionsHost<TOptions> _commandOptionsHost;
    private readonly Func<ILoggerFactory, BuildingOptions, TOptions, IRootCommand> _commandFactory;

    public CommandRunner(
        Command command,
        OptionsHost<LoggingOptions> loggingOptionsHost,
        OptionsHost<BuildingOptions> buildingOptionsHost,
        OptionsHost<TOptions> commandOptionsHost,
        Func<ILoggerFactory, BuildingOptions, TOptions, IRootCommand> commandFactory
    )
    {
        _command = command;
        _loggingOptionsHost = loggingOptionsHost;
        _buildingOptionsHost = buildingOptionsHost;
        _commandOptionsHost = commandOptionsHost;
        _commandFactory = commandFactory;
    }

    public async Task Execute(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var loggingOptions = _loggingOptionsHost.GetValue(parseResult);
        var buildingOptions = _buildingOptionsHost.GetValue(parseResult);
        var commandOptions = _commandOptionsHost.GetValue(parseResult);

        using var loggerFactory = CreateLoggerFactory(loggingOptions);
        var logger = loggerFactory.CreateLogger<Command>();

        VerboseRawOptions(logger, parseResult);
        VerboseParsedOptions(
            logger,
            loggingOptions,
            buildingOptions,
            commandOptions
            );

        var command = _commandFactory(loggerFactory, buildingOptions, commandOptions);

        await command.Execute(cancellationToken);
    }

    private static ILoggerFactory CreateLoggerFactory(LoggingOptions loggingOptions)
    {
        return LoggerFactory.Create(builder =>
        {
            builder.ClearProviders()
                .SetMinimumLevel(GetLogLevel(loggingOptions.Verbosity))
                .AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.IncludeScopes = true;
                    options.ColorBehavior = LoggerColorBehavior.Enabled;
                });
        });

        LogLevel GetLogLevel(Verbosity verbosity)
        {
            return verbosity switch
            {
                Verbosity.Quiet => LogLevel.Error,
                Verbosity.Minimal => LogLevel.Warning,
                Verbosity.Normal => LogLevel.Information,
                Verbosity.Detailed => LogLevel.Debug,
                Verbosity.Diagnostic => LogLevel.Trace,
                _ => LogLevel.None
            };
        }
    }

    private void VerboseRawOptions(ILogger logger, ParseResult parseResult)
    {
        var sb = new StringBuilder();
        sb.Append("Raw options:");

        foreach (var argument in _command.Arguments)
        {
            WriteValue(sb, argument.Name, parseResult.GetResult(argument), false);
        }

        foreach (var option in _command.Options)
        {
            var symbolResult = parseResult.GetResult(option);
            WriteValue(sb, option.Name, symbolResult, symbolResult?.IdentifierTokenCount > 0);
        }

        logger.LogDebug(sb.ToString());

        void WriteValue(StringBuilder sb, string name, SymbolResult? symbolResult, bool defined)
        {
            sb.Append("\n  ");
            sb.Append(name);
            sb.Append(" = ");

            if (symbolResult is null)
            {
                sb.Append("null");
            }
            else if (symbolResult.Tokens.Count > 0)
            {
                sb.Append(string.Join(" ", symbolResult.Tokens.Select(t => t.Value)));
            }
            else if (symbolResult is not null && defined)
            {
                sb.Append('+');
            }
        }
    }

    private static void VerboseParsedOptions(ILogger logger, params IOptions[] items)
    {
        var valueList = new List<KeyValuePair<string, string>>();
        foreach (var options in items)
        {
            options.Verbose(valueList);
        }

        var sb = new StringBuilder();
        sb.Append("Parsed options:");

        foreach (var value in valueList)
        {
            sb.Append("\n  ");
            sb.Append(value.Key);
            sb.Append(" = ");
            sb.Append(value.Value);
        }

        logger.LogDebug(sb.ToString());
    }
}