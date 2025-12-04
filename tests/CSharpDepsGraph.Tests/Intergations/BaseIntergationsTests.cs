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
public class BaseIntergationsTests
{
    private readonly TestLoggerFactory _loggerFactory;

    public BaseIntergationsTests()
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

    public IGraph GetGraph(Action<GraphBuildingOptions>? configure = null)
    {
        var buildingOptions = new GraphBuildingOptions()
        {
            IncludeLinksToPrimitveTypes = true,
            IgnoreLinksToAssemblies = [],
        };

        configure?.Invoke(buildingOptions);

        return new GraphBuilder(_loggerFactory, buildingOptions)
            .Run(ProjectSource.Solution.Projects, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }
}