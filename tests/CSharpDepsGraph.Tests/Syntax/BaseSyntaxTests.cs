using CSharpDepsGraph.Building;
using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Syntax;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Usage",
    "CA1001",
    Justification = "TestLoggerFactory is fake, no need for disposing"
    )]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Usage",
    "NUnit1032"
    )]
public abstract class BaseSyntaxTests
{
    private readonly TestLoggerFactory _loggerFactory;

    protected BaseSyntaxTests()
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

    protected IGraph Build(string sourceText, Action<GraphBuildOptions>? configure = null)
    {
        var options = new GraphBuildOptions()
        {
            FullyQualifiedUid = true,
            CreateLinksToSelf = true,
            CreateLinksToPrimitiveTypes = true,
            AssemblyFilter = [],
        };

        configure?.Invoke(options);

        return GraphFactory.CreateGraph(_loggerFactory, sourceText, options);
    }
}