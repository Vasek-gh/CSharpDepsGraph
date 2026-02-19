using CSharpDepsGraph.Building;
using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Intergations;

public class AssemblyFilterTests : BaseIntergationsTests
{
    [Test]
    public void DefaultAssemblyFilter()
    {
        var graph = GetGraph(o => o.AssemblyFilter = new GraphBuildOptions().AssemblyFilter);

        GraphAssert.HasExactLink(graph, (AsmName.TestProject, "TestProject/TargetFrameworks/StdTypes(int, OperationCanceledException)"),
            ("System.Console_6.0.0.0", "System/Console"),
            ("System.Console_6.0.0.0", "System/Console/ReadKey()"),
            ("System.Console_8.0.0.0", "System/Console"),
            ("System.Console_8.0.0.0", "System/Console/ReadKey()")
        );
    }

    [Test]
    public void All()
    {
        var graph = GetGraph(o => o.AssemblyFilter = ["<all>"]);

        Assert.That(graph.GetNode("External", null).Childs, Is.Empty);
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
    public void MsExtensons()
    {
        var graph = GetGraph(o => o.AssemblyFilter = ["<ms-extensons>"]);

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
    public void ProjectIgnored()
    {
        var graph = GetGraph(o => o.AssemblyFilter = ["TestProject"]);

        GraphAssert.HasLink(graph, (AsmName.TestProjectCli, "Program/<top-level-statements-entry-point>"),
            (AsmName.TestProject, "TestProject/Entities/Car/ctor()")
        );
    }
}