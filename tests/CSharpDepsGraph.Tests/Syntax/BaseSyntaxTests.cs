using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Linq;

namespace CSharpDepsGraph.Tests.Syntax;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Usage",
    "CA1001",
    Justification = "TestLoggerFactory is fake, no need for disposing"
    )]
public class BaseSyntaxTests
{
    private readonly TestLoggerFactory _loggerFactory;

    public BaseSyntaxTests()
    {
        _loggerFactory = new TestLoggerFactory();
    }

    [SetUp]
    public void Init()
    {
        _loggerFactory.Logger.Clear();
    }

    [TearDown]
    public void Done()
    {
        if (_loggerFactory.Logger.Items.Any(e => e.Level >= LogLevel.Warning))
        {
            foreach (var item in _loggerFactory.Logger.Items)
            {
                TestContext.Error.WriteLine($"[{item.Level}] {item.Message}");
            }

            throw new Exception("Detect errors in log");
        }
    }

    protected IGraph Build(string sourceText)
    {
        return GraphFactory.CreateGraph(_loggerFactory, sourceText);
    }
}