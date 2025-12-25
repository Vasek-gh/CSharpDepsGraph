using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Intergations;

public class FullyQualifiedUidGeneratorTests : BaseIntergationsTests
{
    [Test]
    public void LocalAssembly()
    {
        var graph = GetGraph();

        Assert.That(graph.GetNode(AsmName.TestProject, "").Uid, Is.EqualTo("TestProject"));
    }

    [Test]
    public void ExternalAssembly()
    {
        var graph1 = GetGraph((o) => o.DoNotMergeAssembliesWithDifferentVersions = true);
        var links1 = graph1.GetOutgoingLinks(graph1.GetNode(AsmName.TestProject, "TestProject/Constants/IntConst1"));

        Assert.That(links1[0].Target.Uid, Is.EqualTo("netstandard_2.1.0.0/System.int"));
        Assert.That(links1[1].Target.Uid, Is.EqualTo("System.Runtime_6.0.0.0/System.int"));
        Assert.That(links1[2].Target.Uid, Is.EqualTo("System.Runtime_8.0.0.0/System.int"));

        var graph2 = GetGraph((o) => o.DoNotMergeAssembliesWithDifferentVersions = false);
        var links2 = graph2.GetOutgoingLinks(graph2.GetNode(AsmName.TestProject, "TestProject/Constants/IntConst1"));

        Assert.That(links2[0].Target.Uid, Is.EqualTo("netstandard/System.int"));
        Assert.That(links2[1].Target.Uid, Is.EqualTo("System.Runtime/System.int"));
    }
}