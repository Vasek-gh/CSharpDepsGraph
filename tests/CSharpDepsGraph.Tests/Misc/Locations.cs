using System.Linq;
using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Misc;

public class Locations : BaseTests
{
    [Test]
    public void GeneratedDetect()
    {
        var graph = Build();

        Assert.That(!graph.GetNode(AsmName.TestProject, "TestProject.Generated.GeneratedClassPartial").IsGenerated());
        Assert.That(!graph.GetNode(AsmName.TestProject, "TestProject.Generated.GeneratedClassPartial.PartialMethod()").IsGenerated());
        Assert.That(graph.GetNode(AsmName.TestProject, "TestProject.Generated.GeneratedClassPartial.GeneratedMethod()").IsGenerated());

        Assert.That(graph.GetNode(AsmName.TestProject, "TestProject.Generated.GeneratedClass").IsGenerated());
        Assert.That(graph.GetNode(AsmName.TestProject, "TestProject.Generated.GeneratedClass.GeneratedMethod()").IsGenerated());
    }

    [Test]
    public void PartialHasAllLocations()
    {
        var graph = Build(@"
            public partial class Test {
                public partial void TestMethod();
            }
            public partial class Test {
                public partial void TestMethod() {}
            }
        ");

        var testClassNode = graph.GetNode("Test");
        var testMethodNode = graph.GetNode("Test.TestMethod()");
        var testClassNodeLocations = testClassNode.SyntaxLinks.ToArray();
        var testMethodNodeLocations = testMethodNode.SyntaxLinks.ToArray();

        Assert.That(testClassNodeLocations.Length, Is.EqualTo(2));
        Assert.That(testClassNodeLocations[0].GetDisplayString(), Is.EqualTo($"{TestContext.TestFileName}:2:13"));
        Assert.That(testClassNodeLocations[1].GetDisplayString(), Is.EqualTo($"{TestContext.TestFileName}:5:13"));

        Assert.That(testMethodNodeLocations.Length, Is.EqualTo(2));
        Assert.That(testMethodNodeLocations[0].GetDisplayString(), Is.EqualTo($"{TestContext.TestFileName}:3:17"));
        Assert.That(testMethodNodeLocations[1].GetDisplayString(), Is.EqualTo($"{TestContext.TestFileName}:6:17"));
    }
}