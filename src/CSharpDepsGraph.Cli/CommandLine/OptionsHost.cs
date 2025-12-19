using System.CommandLine;

namespace CSharpDepsGraph.Cli.CommandLine;

internal sealed class OptionsHost<T> where T : class, new()
{
    private readonly Command _command;
    private readonly List<Action<T, ParseResult>> _setters;

    public OptionsHost(Command command)
    {
        _command = command;
        _setters = new();
    }

    public OptionsHost<T> AddOption<TValue>(Option<TValue> option, Action<T, TValue?> valueSetter)
    {
        _command.Options.Add(option);
        _setters.Add((o, parseResult) =>
        {
            valueSetter(o, parseResult.GetValue(option));
        });

        return this;
    }

    public OptionsHost<T> AddArgument<TValue>(Argument<TValue> argument, Action<T, TValue?> valueSetter)
    {
        _command.Arguments.Add(argument);
        _setters.Add((o, parseResult) =>
        {
            valueSetter(o, parseResult.GetValue(argument));
        });

        return this;
    }

    public T GetValue(ParseResult parseResult)
    {
        var result = new T();
        foreach (var setter in _setters)
        {
            setter(result, parseResult);
        }

        return result;
    }
}