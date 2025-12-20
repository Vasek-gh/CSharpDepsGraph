using Microsoft.CodeAnalysis;
using NUnit.Framework;
using CSharpDepsGraph.Transforming;
using System.Linq;

namespace CSharpDepsGraph.Tests.Transformation;

[TestFixture]
public class ExternalHideTransformerTests
{
    private const string _nodeId1 = "Id1";
    private const string _nodeId2 = "Id2";
    private const string _externalId1 = "Id3";
    private const string _externalId2 = "Id4";

    [Test]
    public void FullHide()
    {
        var graph = new ExternalHideTransformer(false)
            .Execute(CreateGraph());

        var externalRoot = graph.Root.Childs.SingleOrDefault(n => n.Uid == GraphConsts.ExternalRootNodeId);
        Assert.That(externalRoot, Is.Null);

        var link = graph.Links.Single();
        Assert.That(link.Source.Uid, Is.EqualTo(_nodeId1));
        Assert.That(link.OriginalSource.Uid, Is.EqualTo(_nodeId1));
        Assert.That(link.Target.Uid, Is.EqualTo(_nodeId2));
        Assert.That(link.OriginalTarget.Uid, Is.EqualTo(_nodeId2));
    }

    [Test]
    public void HideOnlyChilds()
    {
        var graph = new ExternalHideTransformer(true)
            .Execute(CreateGraph());

        var node = graph.Root.Childs.Single(n => n.Uid == _nodeId1);
        var externalRoot = graph.Root.Childs.Single(n => n.Uid == GraphConsts.ExternalRootNodeId);

        var inLink = graph.GetIncomingLinks(node).Single();
        Assert.That(inLink.Source.Uid, Is.EqualTo(GraphConsts.ExternalRootNodeId));
        Assert.That(inLink.OriginalSource.Uid, Is.EqualTo(_externalId2));
        Assert.That(inLink.Target.Uid, Is.EqualTo(_nodeId1));
        Assert.That(inLink.OriginalTarget.Uid, Is.EqualTo(_nodeId1));

        var outLinks = graph.GetOutgoingLinks(node);
        Assert.That(outLinks.Length, Is.EqualTo(3));

        var outLink1 = outLinks.Single(l => l.OriginalTarget.Uid == _externalId1);
        Assert.That(outLink1.Target.Uid, Is.EqualTo(GraphConsts.ExternalRootNodeId));

        var outLink2 = outLinks.Single(l => l.OriginalTarget.Uid == _externalId2);
        Assert.That(outLink2.Target.Uid, Is.EqualTo(GraphConsts.ExternalRootNodeId));

        var outLink3 = outLinks.Single(l => l.OriginalTarget.Uid == _nodeId2);
        Assert.That(outLink3.Target.Uid, Is.EqualTo(_nodeId2));
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

    private static NodeMock CreateNode(string id, ISymbol? symbol, params NodeMock[] childs)
    {
        var node = new NodeMock()
        {
            Uid = id,
            Symbol = symbol,
        };

        node.ChildList.AddRange(childs);

        return node;
    }
}