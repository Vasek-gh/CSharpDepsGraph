using System;
using System.Collections.Generic;
using System.Linq;
using CSharpDepsGraph;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Building;

internal class GraphData // todo IGraph and kill Graph.cs
{
    public Node Root { get; }

    public Node External { get; }

    public Dictionary<string, Node> NodeMap { get; }

    public List<Link> Links { get; }

    public GraphData()
    {
        Root = new Node()
        {
            Id = GraphConsts.RootNodeId,
            Symbol = null,
            SyntaxLinks = Array.Empty<SyntaxLink>()
        };

        External = new Node()
        {
            Id = GraphConsts.ExternalRootNodeId,
            Symbol = null,
            SyntaxLinks = Array.Empty<SyntaxLink>()
        };

        Root.ChildList.Add(External);

        NodeMap = new Dictionary<string, Node> {
            { Root.Id, Root },
            { External.Id, External }
        };

        Links = new List<Link>();
    }

    public void AddNode(ILogger logger, Node parent, Node node)
    {
        if (NodeMap.TryGetValue(node.Id, out var existingNode))
        {
            existingNode.LinkedSymbols = MergeLinkedSymbols(existingNode.LinkedSymbols, node.LinkedSymbols);
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

    private static IEnumerable<LinkedSymbol> MergeLinkedSymbols(IEnumerable<LinkedSymbol> a, IEnumerable<LinkedSymbol> b)
    {
        return b.Any()
            ? a.Concat(b).GroupBy(ls => ls.SyntaxLink.GetDisplayString()).Select(g => g.First())
            : a;
    }
}