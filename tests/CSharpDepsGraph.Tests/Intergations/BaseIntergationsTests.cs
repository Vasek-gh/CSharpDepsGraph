using CSharpDepsGraph.Building;
using NUnit.Framework;
using System.Threading;

namespace CSharpDepsGraph.Tests.Intergations;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Usage",
    "CA1001",
    Justification = "TestLoggerFactory is fake, no need for disposing"
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

    public IGraph GetGraph()
    {
        return new GraphBuilder(_loggerFactory)
            .Run(ProjectSource.Solution.Projects, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }
}