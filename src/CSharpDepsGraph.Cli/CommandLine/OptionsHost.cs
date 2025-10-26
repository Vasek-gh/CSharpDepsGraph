using System.CommandLine;
using System.CommandLine.Invocation;

namespace CSharpDepsGraph.Cli.CommandLine;

internal class OptionsHost<T> where T : class
{
    private readonly Command _command;
    private readonly Dictionary<object, object> _optionMap;
    private readonly Func<OptionsHost<T>, T> _factory;

    private readonly bool _init;
    private InvocationContext? _ctx;

    public OptionsHost(Command command, Func<OptionsHost<T>, T> action)
    {
        _command = command;
        _factory = action;
        _optionMap = [];

        _init = false;
        _factory(this);
        _init = true;
    }

    public T GetSettings(InvocationContext cxt)
    {
        _ctx = cxt;

        return _factory(this);
    }

    public TValue? GetOption<TValue>(Func<Option<TValue>> optionFactory)
    {
        CheckInit();

        var option = GetOptionInstance(optionFactory);

        return _ctx == null
            ? default
            : _ctx.ParseResult.GetValueForOption(option);
    }

    public TValue? GetArgument<TValue>(Func<Argument<TValue>> argumenFactory)
    {
        CheckInit();

        var argument = GetArgumentInstance(argumenFactory);

        return _ctx == null
            ? default
            : _ctx.ParseResult.GetValueForArgument(argument);
    }

    private Option<TValue> GetOptionInstance<TValue>(Func<Option<TValue>> optionFactory)
    {
        if (!_optionMap.TryGetValue(optionFactory, out var optionObject))
        {
            CheckInit();

            var option = optionFactory();
            _command.AddOption(option);
            _optionMap.Add(optionFactory, option);
            optionObject = option;
        }

        return optionObject as Option<TValue> ?? throw new InvalidCastException();
    }

    private Argument<TValue> GetArgumentInstance<TValue>(Func<Argument<TValue>> optionFactory)
    {
        if (!_optionMap.TryGetValue(optionFactory, out var optionObject))
        {
            CheckInit();

            var option = optionFactory();
            _command.AddArgument(option);
            _optionMap.Add(optionFactory, option);
            optionObject = option;
        }

        return optionObject as Argument<TValue> ?? throw new InvalidCastException();
    }

    private void CheckInit()
    {
        if (_init && _ctx == null)
        {
            throw new InvalidOperationException();
        }
    }
}
