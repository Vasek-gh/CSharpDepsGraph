using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

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

    public class Entry
    {
        public LogLevel Level { get; set; }
        public required string Message { get; set; }
    }

    public class TestLogger : ILogger
    {
        public List<Entry> Items { get; } = new();

        public void Clear()
        {
            Items.Clear();
        }

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