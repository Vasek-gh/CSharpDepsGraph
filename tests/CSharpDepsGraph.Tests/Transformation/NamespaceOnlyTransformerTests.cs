using Microsoft.CodeAnalysis;
using NUnit.Framework;
using CSharpDepsGraph.Mutation;
using System.Linq;
using System;

namespace CSharpDepsGraph.Tests.Transformation;

[TestFixture]
public class NamespaceOnlyTransformerTests
{
    private const string _assemblyName1 = "A1";
    private const string _assemblyName2 = "A2";
    private const string _assemblyName3 = "A3";
    private const string _namespaceName1 = "N1";
    private const string _namespaceName2 = "N2";
    private const string _namespaceName3 = "N3";

    [Test]
    public void Trivial()
    {
        var graphMock = new GraphMock();
        graphMock.RootNode
            .AddAssemblyNode(_assemblyName1)
            .AddNamespaceNode(_namespaceName1);
        graphMock.RootNode
            .AddAssemblyNode(_assemblyName2)
            .AddNamespaceNode(_namespaceName2);

        var graph = new NamespaceOnlyTransformer().Run(graphMock);

        Assert.That(graph.Root.Id, Is.EqualTo(GraphConsts.RootNodeId));
        Assert.That(graph.Root.Childs.Count(), Is.EqualTo(2));

        var namespaceNode1Check = graph.Root.Childs.First(n => n.Id == _namespaceName1);
        var namespaceNode2Check = graph.Root.Childs.First(n => n.Id == _namespaceName2);

        Assert.That(namespaceNode1Check.Symbol, Is.Null);
        Assert.That(namespaceNode2Check.Symbol, Is.Null);
    }

    [Test]
    public void ChildNamespaceNotTouched()
    {
        var graphMock = new GraphMock();

        var childNode1Mock = graphMock.RootNode
            .AddAssemblyNode(_assemblyName1)
            .AddNamespaceNode(_namespaceName1)
            .AddNamespaceNode(_namespaceName2)
            .AddNode("C1");

        var childNode2Mock = graphMock.RootNode
            .AddAssemblyNode(_assemblyName1)
            .AddNamespaceNode(_namespaceName3)
            .AddNode("C2");

        graphMock.AddLink(Mocks.CreateLink(childNode2Mock, childNode1Mock));

        var graph = new NamespaceOnlyTransformer().Run(graphMock);

        Assert.That(graph.Links.Count(), Is.EqualTo(1));
        Assert.That(graph.Root.Childs.Count(), Is.EqualTo(2));

        var namespaceName1Node = graph.Root.Childs.Single(c => c.Id == _namespaceName1);
        var namespaceName3Node = graph.Root.Childs.Single(c => c.Id == _namespaceName3);

        var link = graph.Links.SingleOrDefault(l =>
            l.Source.Id == _namespaceName3
            && l.Target.Id == _namespaceName1
            && l.OriginalSource.Id == "C2"
            && l.OriginalTarget.Id == "C1"
            );

        Assert.That(link, Is.Not.Null);
    }

    [Test]
    public void GlobalSymbolsMovedToGlobalNamespace()
    {
        var graphMock = new GraphMock();

        var childNode1Mock = graphMock.RootNode
            .AddAssemblyNode(_assemblyName1)
            .AddNode("C1");

        var childNode2Mock = graphMock.RootNode
            .AddAssemblyNode(_assemblyName2)
            .AddNode("C2");

        var childNode3Mock = graphMock.RootNode
            .AddAssemblyNode(_assemblyName1)
            .AddNamespaceNode(_namespaceName1)
            .AddNode("C3");

        graphMock.AddLink(Mocks.CreateLink(childNode3Mock, childNode1Mock));
        graphMock.AddLink(Mocks.CreateLink(childNode3Mock, childNode2Mock));

        var graph = new NamespaceOnlyTransformer().Run(graphMock);

        Assert.That(graph.Root.Childs.Count(), Is.EqualTo(2));

        var namespaceNode = graph.Root.Childs.Single(c => c.Id == _namespaceName1);
        var globalNamespaceNode = graph.Root.Childs.Single(c => c.Id == NamespaceOnlyTransformer.GlobalId);

        var links = graph.Links.Where(l =>
            l.Source.Id == _namespaceName1
            && l.Target.Id == NamespaceOnlyTransformer.GlobalId
            && l.OriginalSource.Id == "C3"
            )
            .ToArray();

        Assert.That(links.Length, Is.EqualTo(2));
        Assert.That(links.SingleOrDefault(l => l.OriginalTarget.Id == "C1"), Is.Not.Null);
        Assert.That(links.SingleOrDefault(l => l.OriginalTarget.Id == "C2"), Is.Not.Null);
    }

    [Test]
    public void OneNameAcrosNodesIsMerged()
    {
        var graphMock = new GraphMock();

        var childNode11 = graphMock.RootNode
            .AddNode("Group", null)
            .AddAssemblyNode(_assemblyName1)
            .AddNamespaceNode(_namespaceName1)
            .AddNode("C1");

        var childNode12 = graphMock.RootNode
            .AddAssemblyNode(_assemblyName2)
            .AddNamespaceNode(_namespaceName1)
            .AddNode("C2");

        var childNode13 = graphMock.RootNode
            .AddNamespaceNode(_namespaceName1)
            .AddNode("C3");

        var childNode2 = graphMock.RootNode
            .AddNamespaceNode(_namespaceName2)
            .AddNode("C4");

        graphMock.LinkList.Add(Mocks.CreateLink(childNode2, childNode11));
        graphMock.LinkList.Add(Mocks.CreateLink(childNode2, childNode12));
        graphMock.LinkList.Add(Mocks.CreateLink(childNode2, childNode13));

        var graph = new NamespaceOnlyTransformer().Run(graphMock);

        Assert.That(graph.Root.Childs.Count(), Is.EqualTo(2));

        var namespaceNode1Check = graph.Root.Childs.First(n => n.Id == _namespaceName1);
        var namespaceNode2Check = graph.Root.Childs.First(n => n.Id == _namespaceName2);

        var links = graph.Links.Where(l =>
            l.Source.Id == _namespaceName2
            && l.Target.Id == _namespaceName1
            && l.OriginalSource.Id == "C4"
            )
            .ToArray();

        Assert.That(links.Length, Is.EqualTo(3));
        Assert.That(links.SingleOrDefault(l => l.OriginalTarget.Id == "C1"), Is.Not.Null);
        Assert.That(links.SingleOrDefault(l => l.OriginalTarget.Id == "C2"), Is.Not.Null);
        Assert.That(links.SingleOrDefault(l => l.OriginalTarget.Id == "C3"), Is.Not.Null);
    }
}