using NUnit.Framework;
using System.Linq;

namespace CSharpDepsGraph.Tests.Intergations;

public class SysLibTests : BaseIntergationsTests
{
    [Test]
    public void CheckDefaultUnitTestConfig()
    {
        var graph = GetGraph();
        var typeNode = graph.GetNode(AsmName.TestProject, "TestProject/TargetFrameworks");
        var methodNode1 = typeNode.Childs.Single(c => c.Symbol?.Name == "Primitive");
        var methodNode2 = typeNode.Childs.Single(c => c.Symbol?.Name == "StdTypes");

        Assert.That(graph.GetOutgoingLinks(methodNode1), Is.Not.Empty);
        Assert.That(graph.GetOutgoingLinks(methodNode2), Is.Not.Empty);
    }

    [Test]
    public void AllSysLibsLinked()
    {
        var graph = GetGraph();

        GraphAssert.HasLink(graph, (AsmName.TestProject, "TestProject/TargetFrameworks/Foo()"),
            (AsmName.Netstandard, "System/int"),
            (AsmName.Runtime60, "System/int"),
            (AsmName.Runtime80, "System/int"),
            (AsmName.Netstandard, "System/DateTime/Now"),
            (AsmName.Runtime60, "System/DateTime/Now"),
            (AsmName.Runtime80, "System/DateTime/Now"),
            (AsmName.Netstandard, "System/Uri/ctor(string)"),
            (AsmName.Runtime60, "System/Uri/ctor(string)"),
            (AsmName.Runtime80, "System/Uri/ctor(string)"),
            (AsmName.TestProject, "TestProject/Entities/Car/ctor()")
        );
    }

    [Test]
    public void PrimitiveArguments()
    {
        var graph = GetGraph();

        var parentNode = graph.GetNode(AsmName.TestProject, "TestProject/TargetFrameworks");

        Assert.That(parentNode.Childs.Count(n => n.Symbol?.Name == "Bar"), Is.EqualTo(1));

        GraphAssert.HasExactLink(graph, (AsmName.TestProject, "TestProject/TargetFrameworks/Bar(int, nint)"),
            (AsmName.Netstandard, "System/int"),
            (AsmName.Netstandard, "System/nint"),
            (AsmName.Runtime60, "System/int"),
            (AsmName.Runtime60, "System/nint"),
            (AsmName.Runtime80, "System/int"),
            (AsmName.Runtime80, "System/nint")
        );
    }

    [Test]
    public void PrimitiveTypesIgnored()
    {
        var graph = GetGraph(o => o.CreateLinksToPrimitiveTypes = false);
        var typeNode = graph.GetNode(AsmName.TestProject, "TestProject/TargetFrameworks");
        var methodNode = typeNode.Childs.Single(c => c.Symbol?.Name == "Primitive");

        Assert.That(graph.GetOutgoingLinks(methodNode), Is.Empty);
    }



    [Test]
    public void MergeAssemblyVersion()
    {
        var graph1 = GetGraph(o => o.SplitAssembliesVersions = false);

        GraphAssert.HasExactLink(graph1, (AsmName.TestProject, "TestProject/Constants/IntConst1"),
            (AsmName.Netstandard, "System/int"),
            (AsmName.Runtime60, "System/int")
        );

        var graph2 = GetGraph(o => o.SplitAssembliesVersions = true);

        GraphAssert.HasExactLink(graph2, (AsmName.TestProject, "TestProject/Constants/IntConst1"),
            (AsmName.Netstandard, "System/int"),
            (AsmName.Runtime60, "System/int"),
            (AsmName.Runtime80, "System/int")
        );
    }
}