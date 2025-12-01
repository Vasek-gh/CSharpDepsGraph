using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Building.Services;

internal class Metrics
{
    private Scope? _scope;
    private readonly List<IMetric> _items;

    public Counter NodeCount { get; private set; }
    public Counter NodeQueryCount { get; private set; }
    public Counter LinkedSymbolCount { get; private set; }
    public Counter LinkedSymbolPrimitiveTypeCount { get; private set; }
    public Counter LinkedSymbolQueryCount { get; private set; }
    public Counter SyntaxLinkCount { get; private set; }
    public Counter SyntaxLinkQueryCount { get; private set; }

    public Value<TimeSpan> ElapsedTime { get; private set; }

    public Metrics()
    {
        _items = new();

        NodeCount = AppendMetric(new Counter("Node count"));
        NodeQueryCount = AppendMetric(new Counter("Node query count"));
        LinkedSymbolCount = AppendMetric(new Counter("Linked symbol count"));
        LinkedSymbolPrimitiveTypeCount = AppendMetric(new Counter("Linked symbol primitive type count"));
        LinkedSymbolQueryCount = AppendMetric(new Counter("Linked symbol query count"));
        SyntaxLinkCount = AppendMetric(new Counter("Syntax link count"));
        SyntaxLinkQueryCount = AppendMetric(new Counter("Syntax link query count"));

        ElapsedTime = AppendMetric(new Value<TimeSpan>("Elapsed time"));
    }

    public void BeginScope(ILogger logger)
    {
        _scope = new Scope()
        {
            Parent = _scope,
            Logger = logger
        };

        foreach (var item in _items)
        {
            item.AppendScope();
        }
    }

    public void EndScope()
    {
        ReportScope();
        _scope = _scope?.Parent;

        foreach (var item in _items)
        {
            item.RemoveScope();
        }
    }

    private void ReportScope()
    {
        if (_scope is null || !_scope.Logger.IsEnabled(LogLevel.Debug))
        {
            return;
        }

        _scope.Logger.LogDebug($"Report:");
        foreach (var item in _items)
        {
            _scope.Logger.LogDebug($"{item.Title}: {item}");
        }
    }

    private T AppendMetric<T>(T metric) where T : IMetric
    {
        _items.Add(metric);
        return metric;
    }

    private class Scope
    {
        public required Scope? Parent { get; set; }
        public required ILogger Logger { get; set; }
    }

    private interface IMetric
    {
        string Title { get; }
        void AppendScope();
        void RemoveScope();
    }

    public class Counter : IMetric
    {
        private readonly List<uint> _values;

        public string Title { get; }

        public Counter(string title)
        {
            _values = new(5);
            Title = title;
        }

        public void Inc()
        {
            for (var i = 0; i < _values.Count; i++)
            {
                _values[i]++;
            }
        }

        public override string? ToString()
        {
            return _values.Count == 0
                ? null
                : _values[_values.Count - 1].ToString(null, null);
        }

        void IMetric.AppendScope()
        {
            _values.Add(0);
        }

        void IMetric.RemoveScope()
        {
            if (_values.Count > 0)
            {
                _values.RemoveAt(_values.Count - 1);
            }
        }
    }

    public class Value<T> : IMetric
    {
        private readonly List<T?> _values;

        public string Title { get; }


        public Value(string title)
        {
            _values = new(5);
            Title = title;

        }

        public override string? ToString()
        {
            return _values.Count == 0
                ? null
                : _values[_values.Count - 1]?.ToString();
        }

        public void Set(T value)
        {
            if (_values.Count > 0)
            {
                _values[_values.Count - 1] = value;
            }
        }

        void IMetric.AppendScope()
        {
            _values.Add(default);
        }

        void IMetric.RemoveScope()
        {
            if (_values.Count > 0)
            {
                _values.RemoveAt(_values.Count - 1);
            }
        }
    }
}