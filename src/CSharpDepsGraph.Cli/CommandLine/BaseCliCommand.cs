using System.CommandLine;
using System.CommandLine.Invocation;
using CSharpDepsGraph.Cli.Commands.Settings;
using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Cli.Commands;
using Microsoft.Extensions.Logging.Console;

namespace CSharpDepsGraph.Cli.CommandLine;

internal abstract class BaseCliCommand : Command
{
    private readonly OptionsHost<BuildSettings> _buildOptionsHost;
    private readonly OptionsHost<LoggingSettings> _loggingOptionsHost;

    public BaseCliCommand(string name, string description)
        : base(name, description)
    {
        this.SetHandler(Execute);

        _buildOptionsHost = new OptionsHost<BuildSettings>(this, (host) =>
        {
            return new BuildSettings
            {
                FileName = host.GetArgument(() => FileNameArgument)?.FullName
                    ?? "todo optional",
                Configuration = host.GetOption(() => ConfigurationOption),
                Properties = host.GetOption(() => PropertiesOption) ?? []
            };
        });

        _loggingOptionsHost = new OptionsHost<LoggingSettings>(this, (host) =>
        {
            return new LoggingSettings
            {
                Verbosity = host.GetOption(() => VerbosityOption),
            };
        });
    }

    private async Task Execute(InvocationContext ctx)
    {
        using var loggerFactory = CreateLoggerFactory(ctx);

        PrintOptions(loggerFactory, ctx);

        var command = CreateCommand(ctx, loggerFactory);
        var mainCommand = CreateMainCommand(ctx, loggerFactory, command);

        await mainCommand.Execute(ctx.GetCancellationToken());
    }

    private ILoggerFactory CreateLoggerFactory(InvocationContext ctx)
    {
        var settings = _loggingOptionsHost.GetSettings(ctx);

        return LoggerFactory.Create(builder =>
        {
            builder.ClearProviders()
                .SetMinimumLevel(GetLogLevel(settings.Verbosity))
                .AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.IncludeScopes = false;
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

    private MainCommand CreateMainCommand(InvocationContext ctx, ILoggerFactory loggerFactory, IGraphCommand command)
    {
        var buildSettings = _buildOptionsHost.GetSettings(ctx);

        return new MainCommand(loggerFactory, buildSettings, command);
    }

    private void PrintOptions(ILoggerFactory loggerFactory, InvocationContext ctx)
    {
        var logger = loggerFactory.CreateLogger("Options");

        foreach (var argument in Arguments)
        {
            var name = argument.Name;
            var value = ctx.ParseResult.GetValueForArgument(argument);
            logger.LogValue(value, name);
        }

        foreach (var option in Options)
        {
            var name = option.Name;
            var value = ctx.ParseResult.GetValueForOption(option);
            logger.LogValue(value, name);
        }
    }

    protected virtual IGraphCommand CreateCommand(InvocationContext ctx, ILoggerFactory loggerFactory)
    {
        throw new NotImplementedException();
    }

    private static Argument<FileInfo> FileNameArgument { get; } = OptionBuilder.Create(() =>
    {
        var description = @"sln or csproj file";

        var fileNameArgument = new Argument<FileInfo>("filename", description);
        fileNameArgument.HelpName = "solution|project";
        fileNameArgument.AddValidator(result =>
        {
            var fileName = result.GetValueForArgument(fileNameArgument).FullName;
            result.ErrorMessage = Utils.GetFileNameError(fileName);
        });

        return fileNameArgument;
    });

    private static Option<Verbosity> VerbosityOption { get; } = OptionBuilder.Create(() =>
    {
        return OptionBuilder.CreateOption<Verbosity>(
            "verbosity",
            "v",
            "level",
            "Sets the verbosity level of the command.",
            LoggingSettings.Defaults.Verbosity,
            [
                ( Verbosity.Quiet, "q", "q[uiet]" ),
                ( Verbosity.Minimal, "m", "m[inimal]" ),
                ( Verbosity.Normal, "n", "n[ormal]" ),
                ( Verbosity.Detailed, "d", "d[etailed]" ),
                ( Verbosity.Diagnostic, "diag", "diag[nostic]" )
            ]
        );
    });

    private static Option<string?> ConfigurationOption { get; } = OptionBuilder.Create(() =>
    {
        return OptionBuilder.CreateOption<string?>(
            "configuration",
            "c",
            "Defines the build configuration."
            );
    });

    private static Option<IEnumerable<KeyValuePair<string, string>>> PropertiesOption { get; } = OptionBuilder.Create(() =>
    {
        var description = @"
            Defines one or more MSBuild properties. Specify multiple properties delimited by
            semicolons or by repeating the option: -p prop1=val1;prop2=val2 or -p prop1=val1 -p prop2=val2.
        ";

        return OptionBuilder.CreateListOption<KeyValuePair<string, string>>(
            "property",
            "p",
            description,
            "name=value",
            argResult =>
            {
                var items = new List<KeyValuePair<string, string>>();

                foreach (var token in argResult.Tokens.SelectMany(t => t.Value.Split(";")))
                {
                    var propParts = token.Split("=").Select(s => s.Trim()).ToArray();
                    if (propParts.Length != 2
                        || string.IsNullOrWhiteSpace(propParts[0])
                        || string.IsNullOrWhiteSpace(propParts[1])
                        )
                    {
                        argResult.ErrorMessage = $"Invalid property format: {token}";
                        return Array.Empty<KeyValuePair<string, string>>();
                    }

                    items.Add(new KeyValuePair<string, string>(propParts[0], propParts[1]));
                }

                return items;
            }
        );
    });
}