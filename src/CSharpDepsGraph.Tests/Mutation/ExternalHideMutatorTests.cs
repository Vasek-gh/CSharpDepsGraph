using CSharpDepsGraph;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using CSharpDepsGraph.Mutation;
using CSharpDepsGraph.Tests.Mocks;
using System.Linq;

namespace CSharpDepsGraph.Tests.Mutation;

[TestFixture]
public class ExternalHideMutatorTests
{
    private const string _nodeId1 = "Id1";
    private const string _nodeId2 = "Id2";
    private const string _externalId1 = "Id3";
    private const string _externalId2 = "Id4";

    [Test]
    public void FullHide()
    {
        var graph = new ExternalHideMutator(false)
            .Run(CreateGraph());

        var externalRoot = graph.Root.Childs.SingleOrDefault(n => n.Id == GraphConsts.ExternalRootNodeId);
        Assert.That(externalRoot, Is.Null);

        var link = graph.Links.Single();
        Assert.That(link.Source.Id, Is.EqualTo(_nodeId1));
        Assert.That(link.OriginalSource.Id, Is.EqualTo(_nodeId1));
        Assert.That(link.Target.Id, Is.EqualTo(_nodeId2));
        Assert.That(link.OriginalTarget.Id, Is.EqualTo(_nodeId2));
    }

    [Test]
    public void HideOnlyChilds()
    {
        var graph = new ExternalHideMutator(true)
            .Run(CreateGraph());

        var node = graph.Root.Childs.Single(n => n.Id == _nodeId1);
        var externalRoot = graph.Root.Childs.Single(n => n.Id == GraphConsts.ExternalRootNodeId);

        var inLink = graph.GetIncomingLinks(node).Single();
        Assert.That(inLink.Source.Id, Is.EqualTo(GraphConsts.ExternalRootNodeId));
        Assert.That(inLink.OriginalSource.Id, Is.EqualTo(_externalId2));
        Assert.That(inLink.Target.Id, Is.EqualTo(_nodeId1));
        Assert.That(inLink.OriginalTarget.Id, Is.EqualTo(_nodeId1));

        var outLinks = graph.GetOutgoingLinks(node);
        Assert.That(outLinks.Count(), Is.EqualTo(3));

        var outLink1 = outLinks.Single(l => l.OriginalTarget.Id == _externalId1);
        Assert.That(outLink1.Target.Id, Is.EqualTo(GraphConsts.ExternalRootNodeId));

        var outLink2 = outLinks.Single(l => l.OriginalTarget.Id == _externalId2);
        Assert.That(outLink2.Target.Id, Is.EqualTo(GraphConsts.ExternalRootNodeId));

        var outLink3 = outLinks.Single(l => l.OriginalTarget.Id == _nodeId2);
        Assert.That(outLink3.Target.Id, Is.EqualTo(_nodeId2));
    }

    private static IGraph CreateGraph()
    {
        var node1 = CreateNode(_nodeId1, null);
        var node2 = CreateNode(_nodeId2, null);
        var externalNode1 = CreateNode(_externalId1, null);
        var externalNode2 = CreateNode(_externalId2, null);

        return Mocks.CreateGraph(
            [
                CreateNode(GraphConsts.ExternalRootNodeId, null,
                    externalNode1,
                    externalNode2
                ),
                node1,
                node2
            ],
            [
                Mocks.CreateLink(node1, externalNode1),
                Mocks.CreateLink(node1, externalNode2),
                Mocks.CreateLink(externalNode2, node1),
                Mocks.CreateLink(node1, node2),
            ]
        );
    }

    private static INode CreateNode(string id, ISymbol? symbol, params INode[] childs)
    {
        return new NodeMock()
        {
            Id = id,
            Symbol = symbol,
            Childs = childs,
            SyntaxLinks = []
        };
    }
}