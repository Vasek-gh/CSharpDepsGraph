using NUnit.Framework;

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
        _loggerFactory.Clear();
    }

    [TearDown]
    public void Done()
    {
        _loggerFactory.Check();
    }

    protected IGraph Build(string sourceText)
    {
        return GraphFactory.CreateGraph(_loggerFactory, sourceText);
    }
}