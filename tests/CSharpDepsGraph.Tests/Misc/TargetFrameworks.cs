using System.IO;
using System.Linq;
using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Misc;

public class TargetFrameworks : BaseTests
{
    [Test]
    public void Main()
    {
        var graph = Build();

        var intTypeNode = graph.GetNode(AsmName.Runtime, "System.int");
        var decimalTypeNode = graph.GetNode(AsmName.Runtime, "System.decimal");
        var longTypeNode = graph.GetNode(AsmName.Netstandard, "System.long");

        CheckMethodLinks(graph, "TestMethod1",
            (intTypeNode, "TestClass.cs:9:17"),
            (longTypeNode, "TestClass.cs:11:17"),
            (decimalTypeNode, "TestClass.cs:13:17")
        );

        CheckMethodLinks(graph, "TestMethod2",
            (longTypeNode, "TestClass.cs:20:17")
        );

        CheckMethodLinks(graph, "TestMethod3",
            (decimalTypeNode, "TestClass.cs:25:17")
        );

        CheckMethodLinks(graph, "TestMethod4",
            (longTypeNode, "TestClass.cs:32:17"),
            (decimalTypeNode, "TestClass.cs:37:17")
        );
    }

    private static void CheckMethodLinks(
        IGraph graph,
        string methodName,
        params (INode node, string location)[] targetLocations
        )
    {
        var methdodNode = graph.GetNode(
            "TestProject.TargetFrameworks",
            $"TestProject.TargetFrameworks.TestClass.{methodName}()"
            );

        var links = graph.GetOutgoingLinks(methdodNode).ToArray();

        Assert.That(links.Length, Is.EqualTo(targetLocations.Length));
        foreach (var targetLocation in targetLocations)
        {
            Assert.That(GetLinkShortLocation(links, targetLocation.node), Is.EqualTo(targetLocation.location));
        }
    }

    private static string GetLinkShortLocation(ILink[] links, INode target)
    {
        return links.Single(l => l.Target.Id == target.Id)
            .SyntaxLink
            .GetDisplayString()
            .Split(Path.DirectorySeparatorChar)
            .Last();
    }
}