using NUnit.Framework;

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
}