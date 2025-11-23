using NUnit.Framework;
using System.Linq;

namespace CSharpDepsGraph.Tests.Intergations;

public class SysLibTests : BaseIntergationsTests
{
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
}