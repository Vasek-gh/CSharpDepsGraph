using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Cli.Commands;
using CSharpDepsGraph.Cli.Options;
using System.CommandLine;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace CSharpDepsGraph.Cli.CommandLine;

internal class CommandBuilder<TOptions>
    where TOptions : class, IOptions, new()
{
    private string? _name;
    private string? _description;
    private Func<ILoggerFactory, BuildingOptions, TOptions, IRootCommand>? _commandFactory;
    private readonly List<Action<OptionsHost<TOptions>>> _commandOptionsActions;

    public CommandBuilder()
    {
        _commandOptionsActions = new();
    }

    public CommandBuilder<TOptions> WithName(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        SetValue(ref _name, value);
        return this;
    }

    public CommandBuilder<TOptions> WithDescription(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        SetValue(ref _description, value);
        return this;
    }

    public CommandBuilder<TOptions> WithFactory(Func<ILoggerFactory, BuildingOptions, TOptions, IRootCommand> value)
    {
        SetValue(ref _commandFactory, value);
        return this;
    }

    public CommandBuilder<TOptions> AddOption<TValue>(Option<TValue> option, Action<TOptions, TValue?> valueSetter)
    {
        _commandOptionsActions.Add(oh => oh.AddOption(option, valueSetter));
        return this;
    }

    public Command Build()
    {
        Validate();

        var command = new Command(_name, _description);

        var loggingOptionsHost = CreateLoggingOptionsHost(command);
        var buildingOptionsHost = CreateBuildingOptionsHost(command);
        var commandOptionsHost = CreateCommandOptionsHost(command);

        command.SetAction((pr, ct) =>
        {
            return new CommandRunner<TOptions>(
                command,
                loggingOptionsHost,
                buildingOptionsHost,
                commandOptionsHost,
                _commandFactory
                )
                .Execute(pr, ct);
        });

        return command;
    }

    private static OptionsHost<LoggingOptions> CreateLoggingOptionsHost(Command command)
    {
        return new OptionsHost<LoggingOptions>(command)
            .AddOption(RootOptions.VerbosityOption, (o, v) => o.Verbosity = v);
    }

    private static OptionsHost<BuildingOptions> CreateBuildingOptionsHost(Command command)
    {
        return new OptionsHost<BuildingOptions>(command)
            .AddRequiredArgument(RootOptions.FileNameArgument, (o, v) => o.FileName = v.FullName)
            .AddOption(RootOptions.PropertiesOption, (o, v) => o.Properties = v ?? [])
            .AddOption(RootOptions.ParseGeneratedCodeOption, (o, v) => o.GraphOptions.ParseGeneratedCode = v)
            .AddOption(RootOptions.CreateLinksToSelfOption, (o, v) => o.GraphOptions.CreateLinksToSelf = v)
            .AddOption(RootOptions.CreateLinksToPrimitiveTypesOption, (o, v) => o.GraphOptions.CreateLinksToPrimitiveTypes = v)
            .AddOption(RootOptions.SplitAssembliesVersionsOption, (o, v) => o.GraphOptions.SplitAssembliesVersions = v);
    }

    private OptionsHost<TOptions> CreateCommandOptionsHost(Command command)
    {
        var host = new OptionsHost<TOptions>(command);
        foreach (var optionAction in _commandOptionsActions)
        {
            optionAction(host);
        }

        return host;
    }

    [MemberNotNull(nameof(_name))]
    [MemberNotNull(nameof(_description))]
    [MemberNotNull(nameof(_commandFactory))]
    private void Validate()
    {
        if (_name is null)
        {
            throw new InvalidOperationException("Empty command name");
        }

        if (_description is null)
        {
            throw new InvalidOperationException("Empty command description");
        }

        if (_commandFactory is null)
        {
            throw new InvalidOperationException("Empty command factory");
        }

        if (_commandOptionsActions.Count == 0)
        {
            throw new InvalidOperationException("Empty command options");
        }
    }

    private static void SetValue<T>(ref T? field, T value, [CallerArgumentExpression(nameof(field))] string fieldName = "")
    {
        if (field is not null)
        {
            throw new InvalidOperationException($"Member {fieldName} already set");
        }

        field = value;
    }
}