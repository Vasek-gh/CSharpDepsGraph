using NUnit.Framework;
using System.Linq;

namespace CSharpDepsGraph.Tests.Intergations;

public class GeneratedCodeTests : BaseIntergationsTests
{
    [Test]
    public void GeneratedDetect()
    {
        var graph = GetGraph();

        Assert.That(!IsGenerated(graph.GetNode(AsmName.TestProject, "TestProject/Generated/GeneratedClassPartial")));
        Assert.That(!IsGenerated(graph.GetNode(AsmName.TestProject, "TestProject/Generated/GeneratedClassPartial/PartialMethod()")));
        Assert.That(IsGenerated(graph.GetNode(AsmName.TestProject, "TestProject/Generated/GeneratedClassPartial/GeneratedMethod()")));

        Assert.That(IsGenerated(graph.GetNode(AsmName.TestProject, "TestProject/Generated/GeneratedClass")));
        Assert.That(IsGenerated(graph.GetNode(AsmName.TestProject, "TestProject/Generated/GeneratedClass/GeneratedMethod()")));
    }

    public static bool IsGenerated(INode node)
    {
        return node.SyntaxLinks.All(sl => sl.LocationKind == LocationKind.Generated);
    }
}