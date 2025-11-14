using System.IO;
using System.Linq;
using CSharpDepsGraph.Building.Entities;
using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Intergations;

public class PreprocessorDirectivesTests : BaseIntergationsTests
{
    [Test]
    public void TypeOptinalBody()
    {
        var graph = GetGraph();

        var intTypeNode = graph.GetNode(AsmName.Runtime, "System/int");
        var decimalTypeNode = graph.GetNode(AsmName.Runtime, "System/decimal");
        var longTypeNode = graph.GetNode(AsmName.Netstandard, "System/long");

        CheckMethodLinks(graph, "Preprocessor1.TestMethod1",
            (intTypeNode, "Preprocessor1.cs:9:17"),
            (longTypeNode, "Preprocessor1.cs:11:17"),
            (decimalTypeNode, "Preprocessor1.cs:13:17")
        );

        CheckMethodLinks(graph, "Preprocessor1.TestMethod2",
            (longTypeNode, "Preprocessor1.cs:20:17")
        );

        CheckMethodLinks(graph, "Preprocessor1.TestMethod3",
            (decimalTypeNode, "Preprocessor1.cs:25:17")
        );

        CheckMethodLinks(graph, "Preprocessor1.TestMethod4",
            (longTypeNode, "Preprocessor1.cs:32:17"),
            (decimalTypeNode, "Preprocessor1.cs:37:17")
        );
    }

    [Test]
    public void TypeOptinalDeclaration()
    {
        var graph = GetGraph();

        Assert.That(
            graph.GetNode(AsmName.TestProject, "TestProject")
                .Childs
                .Count(n => n.Symbol?.Name.StartsWith("Preprocessor2") == true),
            Is.EqualTo(1)
        );

        var node = graph.GetNode(AsmName.TestProject, "TestProject/Preprocessor2<T>");
        var nodeCar = graph.GetNode(AsmName.TestProject, "TestProject/Entities/Car");
        var nodeAirplane = graph.GetNode(AsmName.TestProject, "TestProject/Entities/Airplane");

        var links = graph.GetOutgoingLinks(node).ToArray();

        Assert.That(links.Length, Is.EqualTo(2));
        Assert.That(links.SingleOrDefault(l => l.Target.Id == nodeCar.Id), Is.Not.Null);
        Assert.That(links.SingleOrDefault(l => l.Target.Id == nodeAirplane.Id), Is.Not.Null);
    }

    [Test]
    public void TypeDifferentDeclaration()
    {
        var graph = GetGraph();

        var userMethodNode = graph.GetNode(AsmName.TestProject, "TestProject/Preprocessor3User/Test()");

        var nodeType1 = graph.GetNode(AsmName.TestProject, "TestProject/Preprocessor3_1");
        var nodeMethod1 = graph.GetNode(AsmName.TestProject, "TestProject/Preprocessor3_1/Foo()");

        var nodeType2 = graph.GetNode(AsmName.TestProject, "TestProject/Preprocessor3_2");
        var nodeMethod2 = graph.GetNode(AsmName.TestProject, "TestProject/Preprocessor3_2/Foo()");

        var links = graph.GetOutgoingLinks(userMethodNode).ToArray();
        Assert.That(links.Length, Is.EqualTo(4));
        Assert.That(links.SingleOrDefault(l => l.Target.Id == nodeType1.Id), Is.Not.Null);
        Assert.That(links.SingleOrDefault(l => l.Target.Id == nodeMethod1.Id), Is.Not.Null);
        Assert.That(links.SingleOrDefault(l => l.Target.Id == nodeType1.Id), Is.Not.Null);
        Assert.That(links.SingleOrDefault(l => l.Target.Id == nodeMethod2.Id), Is.Not.Null);
    }

    private static void CheckMethodLinks(
        IGraph graph,
        string methodName,
        params (INode node, string location)[] targetLocations
        )
    {
        var methdodNode = graph.GetNode(
            AsmName.TestProject,
            $"TestProject.{methodName}()"
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