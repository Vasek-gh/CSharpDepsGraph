using CSharpDepsGraph.Building;
using NUnit.Framework;
using System;
using System.Threading;

namespace CSharpDepsGraph.Tests.Intergations;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Usage",
    "CA1001",
    Justification = "TestLoggerFactory is fake, no need for disposing"
    )]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Usage",
    "NUnit1032"
    )]
public abstract class BaseIntergationsTests
{
    private readonly TestLoggerFactory _loggerFactory;

    protected BaseIntergationsTests()
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

    public IGraph GetGraph(Action<GraphBuildOptions>? configure = null)
    {
        var options = new GraphBuildOptions()
        {
            IncludeLinksToPrimitiveTypes = true,
            GenerateFullyQualifiedUid = true,
            IgnoreLinksToAssemblies = [],
            SplitAssembliesVersions = true,
        };

        configure?.Invoke(options);

        return new GraphBuilder(_loggerFactory, options)
            .Run(ProjectSource.Solution.Projects, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }
}