using System.CommandLine;
using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Cli.Commands;
using Microsoft.Extensions.Logging.Console;
using CSharpDepsGraph.Cli.Options;
using CSharpDepsGraph.Building;
using System.CommandLine.Parsing;

namespace CSharpDepsGraph.Cli.CommandLine;

internal abstract class BaseCliCommand : Command
{
    private readonly OptionsHost<BuildOptions> _buildOptionsHost;
    private readonly OptionsHost<GraphBuildOptions> _graphBuildOptionsHost;
    private readonly OptionsHost<LoggingOptions> _loggingOptionsHost;

    public BaseCliCommand(string name, string description)
        : base(name, description)
    {
        SetAction(Execute);

        _loggingOptionsHost = new OptionsHost<LoggingOptions>(this)
            .AddOption(VerbosityOption, (o, v) => o.Verbosity = v);

        _buildOptionsHost = new OptionsHost<BuildOptions>(this)
            .AddOption(PropertiesOption, (o, v) => o.Properties = v ?? [])
            .AddRequiredArgument(FileNameArgument, (o, v) => o.FileName = v.FullName);

        _graphBuildOptionsHost = new OptionsHost<GraphBuildOptions>(this)
            .AddOption(ParseGeneratedCodeOption, (o, v) => o.ParseGeneratedCode = v)
            .AddOption(CreateLinksToSelfOption, (o, v) => o.CreateLinksToSelf = v)
            .AddOption(CreateLinksToPrimitiveTypesOption, (o, v) => o.CreateLinksToPrimitiveTypes = v)
            .AddOption(SplitAssembliesVersionsOption, (o, v) => o.SplitAssembliesVersions = v);
    }

    private async Task Execute(ParseResult parseResult, CancellationToken cancellationToken)
    {
        using var loggerFactory = CreateLoggerFactory(parseResult);

        PrintOptions(loggerFactory, parseResult);

        var buildOptions = _buildOptionsHost.GetValue(parseResult);
        var graphBuildOptions = _graphBuildOptionsHost.GetValue(parseResult);
        buildOptions.GraphOptions = graphBuildOptions;

        var logger = loggerFactory.CreateLogger(GetType().Name);
        logger.Verbose(buildOptions);
        logger.Verbose(graphBuildOptions);
        BeforeExecute(logger, parseResult);

        var command = CreateCommand(parseResult, loggerFactory, buildOptions);

        await command.Execute(cancellationToken);
    }

    protected virtual void BeforeExecute(ILogger logger, ParseResult parseResult)
    {
        throw new NotImplementedException();
    }

    protected virtual ICommand CreateCommand(
        ParseResult parseResult,
        ILoggerFactory loggerFactory,
        BuildOptions buildOptions
        )
    {
        throw new NotImplementedException();
    }

    private ILoggerFactory CreateLoggerFactory(ParseResult parseResult)
    {
        var settings = _loggingOptionsHost.GetValue(parseResult);

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

    private void PrintOptions(ILoggerFactory loggerFactory, ParseResult parseResult)
    {
        var cliOptionsLogger = loggerFactory.CreateLogger("CliOptions");

        foreach (var argument in Arguments)
        {
            LogResult(argument.Name, parseResult.GetResult(argument), false);
        }

        foreach (var option in Options)
        {
            var result = parseResult.GetResult(option);
            LogResult(option.Name, parseResult.GetResult(option), result?.IdentifierTokenCount > 0);
        }

        void LogResult(string name, SymbolResult? symbolResult, bool defined)
        {
            var value = "";
            if (symbolResult is null)
            {
                value = "null";
            }
            else if (symbolResult.Tokens.Count > 0)
            {
                value = string.Join(" ", symbolResult.Tokens.Select(t => t.Value));
            }
            else if (symbolResult is not null && defined)
            {
                value = "+";
            }

            cliOptionsLogger.LogValue(value, name);
        }
    }

    private static Argument<FileInfo> FileNameArgument { get; } = OptionBuilder.Create(() =>
    {
        var description = @"sln or slnx file";

        var fileNameArgument = new Argument<FileInfo>("filename");
        fileNameArgument.Description = description;
        fileNameArgument.HelpName = "solution";
        fileNameArgument.Validators.Add(result =>
        {
            var fileName = result.GetRequiredValue(fileNameArgument).FullName;
            var error = OptionsUtils.GetFileNameError(fileName);
            if (error is not null)
            {
                result.AddError(error);
            }
        });

        return fileNameArgument;
    });

    private static Option<Verbosity> VerbosityOption { get; } = OptionBuilder.Create(() =>
    {
        return OptionBuilder.CreateEnumOption<Verbosity>(
            "verbosity",
            "v",
            "level",
            "Sets the verbosity level of the command.",
            Verbosity.Normal,
            [
                // cSpell:disable
                ( Verbosity.Quiet, "q", "q[uiet]" ),
                ( Verbosity.Minimal, "m", "m[inimal]" ),
                ( Verbosity.Normal, "n", "n[ormal]" ),
                ( Verbosity.Detailed, "d", "d[etailed]" ),
                ( Verbosity.Diagnostic, "diag", "diag[nostic]" )
                // cSpell:enable
            ]
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
                        return ([], $"Invalid property format: {token}");
                    }

                    items.Add(new KeyValuePair<string, string>(propParts[0], propParts[1]));
                }

                return (items, null);
            }
        );
    });

    private static Option<bool> ParseGeneratedCodeOption { get; } = OptionBuilder.Create(() =>
    {
        return OptionBuilder.CreateOption<bool>(
            "parse-generated",
            null,
            "Parse the generated code not located in intermediate output path"
            );
    });

    private static Option<bool> CreateLinksToSelfOption { get; } = OptionBuilder.Create(() =>
    {
        return OptionBuilder.CreateOption<bool>(
            "links-to-self",
            null,
            "Include references to symbols from your own type"
            );
    });

    private static Option<bool> CreateLinksToPrimitiveTypesOption { get; } = OptionBuilder.Create(() =>
    {
        return OptionBuilder.CreateOption<bool>(
            "links-to-primitives",
            null,
            "Include links to symbols of primitive types"
            );
    });

    private static Option<bool> SplitAssembliesVersionsOption { get; } = OptionBuilder.Create(() =>
    {
        return OptionBuilder.CreateOption<bool>(
            "split-asm-versions",
            null,
            "Create separate nodes for each version of an assembly"
            );
    });
}