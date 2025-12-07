using CSharpDepsGraph.Building;
using NUnit.Framework;
using System;

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

    protected IGraph Build(string sourceText, Action<GraphBuildOptions>? configure = null)
    {
        var options = new GraphBuildOptions()
        {
            IncludeLinksToPrimitveTypes = true,
            IgnoreLinksToAssemblies = [],
        };

        configure?.Invoke(options);

        return GraphFactory.CreateGraph(_loggerFactory, sourceText, options);
    }
}