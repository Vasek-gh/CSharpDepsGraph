using CSharpDepsGraph.Building;
using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Integrations;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Usage",
    "CA1001",
    Justification = "TestLoggerFactory is fake, no need for disposing"
    )]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Usage",
    "NUnit1032"
    )]
public abstract class BaseIntegrationsTests
{
    private readonly TestLoggerFactory _loggerFactory;

    protected BaseIntegrationsTests()
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
            FullyQualifiedUid = true,
            CreateLinksToSelf = true,
            CreateLinksToPrimitiveTypes = true,
            SplitAssembliesVersions = true,
            AssemblyFilter = [],
        };

        configure?.Invoke(options);

        return new GraphBuilder(_loggerFactory, options)
            .Run(ProjectSource.Solution.Projects, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }
}