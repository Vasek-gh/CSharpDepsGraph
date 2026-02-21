using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace CSharpDepsGraph.Tests;

public class TestLoggerFactory : ILoggerFactory
{
    public TestLogger Logger { get; } = new();

    public void Dispose()
    {
    }

    public void AddProvider(ILoggerProvider provider)
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        return Logger;
    }

    public void Clear()
    {
        Logger.Items.Clear();
    }

    public void Check()
    {
        foreach (var item in Logger.Items)
        {
            TestContext.Error.WriteLine($"[{item.Level}] {item.Message}");
        }

        if (Logger.Items.Count > 0)
        {
            throw new Exception("Detect errors in log");
        }
    }

    public class Entry
    {
        public LogLevel Level { get; set; }
        public required string Message { get; set; }
    }

    public class TestLogger : ILogger
    {
        public List<Entry> Items { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel.Warning;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            Items.Add(new Entry()
            {
                Level = logLevel,
                Message = formatter(state, exception)
            });
        }
    }
}