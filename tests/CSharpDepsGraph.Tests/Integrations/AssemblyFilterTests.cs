using CSharpDepsGraph.Building;
using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Integrations;

public class AssemblyFilterTests : BaseIntegrationsTests
{
    [Test]
    public void All()
    {
        var graph = GetGraph(o => o.AssemblyFilter = ["<all>"]);
        var assemblies = graph.Root.Childs.Where(
            n => n.SyntaxLinks.Any(sl => sl.LocationKind == LocationKind.External)
        );

        Assert.That(assemblies, Is.Empty);
    }

    [Test]
    public void StdLib()
    {
        var graph = GetGraph(o => o.AssemblyFilter = ["<std-lib>"]);

        GraphAssert.HasNotSymbol(graph, ("System.Runtime_8.0.0.0", null));
        GraphAssert.HasSymbol(graph, ("System.CommandLine_2.0.1.0", null));
        GraphAssert.HasSymbol(graph, ("Microsoft.CodeAnalysis_4.7.0.0", null));
        GraphAssert.HasSymbol(graph, ("Microsoft.Extensions.Logging.Abstractions_9.0.0.0", null));
    }

    [Test]
    public void Sys()
    {
        var graph = GetGraph(o => o.AssemblyFilter = ["<sys>"]);

        GraphAssert.HasNotSymbol(graph, ("System.Runtime_8.0.0.0", null));
        GraphAssert.HasNotSymbol(graph, ("System.CommandLine_2.0.1.0", null));
        GraphAssert.HasSymbol(graph, ("Microsoft.CodeAnalysis_4.7.0.0", null));
        GraphAssert.HasSymbol(graph, ("Microsoft.Extensions.Logging.Abstractions_9.0.0.0", null));
    }

    [Test]
    public void Ms()
    {
        var graph = GetGraph(o => o.AssemblyFilter = ["<ms>"]);

        GraphAssert.HasSymbol(graph, ("System.Runtime_8.0.0.0", null));
        GraphAssert.HasSymbol(graph, ("System.CommandLine_2.0.1.0", null));
        GraphAssert.HasNotSymbol(graph, ("Microsoft.CodeAnalysis_4.7.0.0", null));
        GraphAssert.HasNotSymbol(graph, ("Microsoft.Extensions.Logging.Abstractions_9.0.0.0", null));
    }

    [Test]
    public void MsExtensions()
    {
        var graph = GetGraph(o => o.AssemblyFilter = ["<ms-extensions>"]);

        GraphAssert.HasSymbol(graph, ("System.Runtime_8.0.0.0", null));
        GraphAssert.HasSymbol(graph, ("System.CommandLine_2.0.1.0", null));
        GraphAssert.HasSymbol(graph, ("Microsoft.CodeAnalysis_4.7.0.0", null));
        GraphAssert.HasNotSymbol(graph, ("Microsoft.Extensions.Logging.Abstractions_9.0.0.0", null));
    }

    [Test]
    public void Custom()
    {
        var graph = GetGraph(o => o.AssemblyFilter = ["System.Command*", "*.CodeAnalysis"]);

        GraphAssert.HasSymbol(graph, ("System.Runtime_8.0.0.0", null));
        GraphAssert.HasNotSymbol(graph, ("System.CommandLine_2.0.1.0", null));
        GraphAssert.HasNotSymbol(graph, ("Microsoft.CodeAnalysis_4.7.0.0", null));
        GraphAssert.HasSymbol(graph, ("Microsoft.Extensions.Logging.Abstractions_9.0.0.0", null));
    }

    [Test]
    public void Default()
    {
        var graph = GetGraph(o => o.AssemblyFilter = new GraphBuildOptions().AssemblyFilter);

        GraphAssert.HasSymbol(graph, ("System.Runtime_8.0.0.0", null));
        GraphAssert.HasSymbol(graph, ("System.CommandLine_2.0.1.0", null));
        GraphAssert.HasSymbol(graph, ("Microsoft.CodeAnalysis_4.7.0.0", null));
        GraphAssert.HasSymbol(graph, ("Microsoft.Extensions.Logging.Abstractions_9.0.0.0", null));
    }

    [Test]
    public void ProjectIgnored()
    {
        var graph = GetGraph(o => o.AssemblyFilter = ["TestProject"]);

        GraphAssert.HasLink(graph, (AsmName.TestProjectCli, "Program/<top-level-statements-entry-point>"),
            (AsmName.TestProject, "TestProject/Entities/Car/ctor()")
        );
    }
}