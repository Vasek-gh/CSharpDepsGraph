using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Building;

internal class GraphData
{
    public Node Root { get; }

    public Node External { get; }

    public Dictionary<string, Node> NodeMap { get; }

    public List<Link> Links { get; }

    public GraphData()
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
    }

    public void AddNode(ILogger logger, Node parent, Node node)
    {
        if (NodeMap.TryGetValue(node.Id, out var existingNode))
        {
            AddLinkedSymbols(existingNode.LinkedSymbolsList, node.LinkedSymbolsList);
            return;
        }

        if (!NodeMap.TryGetValue(parent.Id, out var existingParentNode))
        {
            logger.LogWarning($"""
                Detected attempt add node, parent which not present in map.
                Parent id: {parent.Id}.
                Node id: {node.Id}.
                """
            );

            return;
        }

        existingParentNode.ChildList.Add(node);
        NodeMap.Add(node.Id, node);
    }

    public Node? AddNode(
        ILogger logger,
        string parentId,
        string id,
        ISymbol symbol
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

        node = new Node(id, symbol)
        {
            LinkedSymbolsList = []
        };

        NodeMap.Add(node.Id, node);
        parentNode.ChildList.Add(node);

        return node;
    }

    private static void AddLinkedSymbols(List<LinkedSymbol> to, IEnumerable<LinkedSymbol> from)
    {
        var uniqueFrom = from.Where(fromItem =>
        {
            var fromItemSyntaxLink = fromItem.SyntaxLink;

            foreach (var toItem in to)
            {
                var toItemSyntaxLink = toItem.SyntaxLink;
                if (toItemSyntaxLink.Path == fromItemSyntaxLink.Path
                    && toItemSyntaxLink.Line == fromItemSyntaxLink.Line
                    && toItemSyntaxLink.Column == fromItemSyntaxLink.Column
                    )
                {
                    return false;
                }
            }

            return true;
        });

        to.AddRange(uniqueFrom);
    }
}