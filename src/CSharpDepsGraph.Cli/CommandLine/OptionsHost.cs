using System.CommandLine;
using System.CommandLine.Invocation;

namespace CSharpDepsGraph.Cli.CommandLine;

internal sealed class OptionsHost<T> where T : class, new()
{
    private readonly Command _command;
    private readonly List<Action<T, InvocationContext>> _setters;

    public OptionsHost(Command command)
    {
        _command = command;
        _setters = new();
    }

    public OptionsHost<T> AddOption<TValue>(Option<TValue> option, Action<T, TValue?> valueSetter)
    {
        _command.AddOption(option);
        _setters.Add((o, ctx) =>
        {
            valueSetter(o, ctx.ParseResult.GetValueForOption(option));
        });

        return this;
    }

    public OptionsHost<T> AddArgument<TValue>(Argument<TValue> argument, Action<T, TValue?> valueSetter)
    {
        _command.AddArgument(argument);
        _setters.Add((o, ctx) =>
        {
            valueSetter(o, ctx.ParseResult.GetValueForArgument(argument));
        });

        return this;
    }

    public T GetValue(InvocationContext ctx)
    {
        var result = new T();
        foreach (var setter in _setters)
        {
            setter(result, ctx);
        }

        return result;
    }
}