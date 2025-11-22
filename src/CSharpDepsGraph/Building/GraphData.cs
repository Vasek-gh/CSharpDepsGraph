using CSharpDepsGraph.Building.Entities;
using CSharpDepsGraph.Building.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Building;

internal class GraphData
{
    private readonly Counters _counters;
    private readonly SymbolComparer _symbolComparer;
    private readonly ISymbolIdGenerator _symbolIdGenerator;

    public Node Root { get; }

    public Node External { get; }

    public Dictionary<string, Node> NodeMap { get; }

    public List<Link> Links { get; }

    public GraphData(Counters counters, SymbolComparer symbolComparer, ISymbolIdGenerator symbolIdGenerator)
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
        _symbolComparer = symbolComparer;
        _symbolIdGenerator = symbolIdGenerator;
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
            if (!_symbolComparer.Compare(symbol, node.Symbol!))
            {
                _symbolComparer.Compare(symbol, node.Symbol!);
                // todo kill
            }
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

    public Node AddNode(
        Node parent,
        ISymbol symbol
        )
    {
        var child = parent.ChildList.FirstOrDefault(c => _symbolComparer.Compare(c.Symbol, symbol));
        if (child is null)
        {
            var id = _symbolIdGenerator.Execute(symbol);
            child = new Node(id, symbol, null)
            {
                LinkedSymbolsList = []
            };
        }

        return child;
    }
}