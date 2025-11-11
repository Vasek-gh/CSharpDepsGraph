using System.Collections.Generic;
using CSharpDepsGraph.Building.Entities;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Building;

internal class GraphData
{
    private readonly Counters _counters;

    public Node Root { get; }

    public Node External { get; }

    public Dictionary<string, Node> NodeMap { get; }

    public List<Link> Links { get; }

    public GraphData(Counters counters)
    {
        Root = new Node(GraphConsts.RootNodeId, null)
        {
            LinkedSymbolsList = []
        };

        External = new Node(GraphConsts.ExternalRootNodeId, null)
        {
            LinkedSymbolsList = []
        };

        Root.ChildList.Add(External);

        NodeMap = new Dictionary<string, Node> {
            { Root.Id, Root },
            { External.Id, External }
        };

        Links = new List<Link>(5_000);
        _counters = counters;
    }

    public Node? AddNode(
        ILogger logger,
        string parentId,
        string id,
        ISymbol symbol,
        List<INodeSyntaxLink>? syntaxLinks = null
        )
    {
        if (NodeMap.TryGetValue(id, out var node))
        {
            return node;
        }

        if (!NodeMap.TryGetValue(parentId, out var parentNode))
        {
            logger.LogWarning($"""
                Detected attempt add node, parent which not present in map.
                Parent id: {parentId}.
                Node id: {id}.
                """
            );

            return null;
        }

        node = new Node(id, symbol, syntaxLinks)
        {
            LinkedSymbolsList = []
        };

        NodeMap.Add(node.Id, node);
        parentNode.ChildList.Add(node);
        _counters.AddNode();

        return node;
    }
}