using CSharpDepsGraph.Building;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Linq;
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
        _loggerFactory.Logger.Clear();
    }

    [TearDown]
    public void Done()
    {
        if (_loggerFactory.Logger.Items.Any(e => e.Level >= LogLevel.Warning))
        {
            foreach (var item in _loggerFactory.Logger.Items)
            {
                NUnit.Framework.TestContext.Error.WriteLine($"[{item.Level}] {item.Message}");
            }

            throw new Exception("Detect errors in log");
        }
    }

    public IGraph GetGraph()
    {
        return new GraphBuilder(_loggerFactory)
            .Run(ProjectSource.Solution.Projects, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }
}