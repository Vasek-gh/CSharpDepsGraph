using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using CSharpDepsGraph.Building;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CSharpDepsGraph.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Usage",
    "CA1001",
    Justification = "TestLoggerFactory is fake, no need for disposing"
    )]
public class BaseTests
{
    private readonly TestLoggerFactory _loggerFactory;

    public BaseTests()
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

    protected IGraph Build(string? sourceText = null)
    {
        return BuildAsync(sourceText, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }

    protected async Task<IGraph> BuildAsync(string? sourceText = null, CancellationToken cancellationToken = default)
    {
        var solution = await TestContext.Instance.CreateSolutionAsync(sourceText, cancellationToken);

        return await new GraphBuilder(_loggerFactory)
            .Run(solution.Projects, CancellationToken.None);
    }
}