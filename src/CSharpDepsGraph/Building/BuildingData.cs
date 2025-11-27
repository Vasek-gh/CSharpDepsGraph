using CSharpDepsGraph.Building.Entities;
using CSharpDepsGraph.Building.Generators;
using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building;

internal class BuildingData
{
    private readonly Counters _counters;
    private readonly SymbolComparer _symbolComparer;
    private readonly ISymbolUidGenerator _symbolUidGenerator;

    public Node Root { get; }

    public Node External { get; }

    public List<Link> Links { get; }

    public BuildingData(Counters counters, SymbolComparer symbolComparer, ISymbolUidGenerator symbolIdGenerator)
    {
        _counters = counters;
        _symbolComparer = symbolComparer;
        _symbolUidGenerator = symbolIdGenerator;

        External = new Node(GraphConsts.ExternalRootNodeId, null);

        Root = new Node(GraphConsts.RootNodeId, null);
        Root.ChildList = AddNodeListItem(Root.ChildList, External);

        Links = new();
    }

    public Node AddChildNode(
        Node parent,
        ISymbol symbol
        )
    {
        return AddChildNode(parent, symbol, out var _);
    }

    public Node AddChildNode(
        Node parent,
        ISymbol symbol,
        out bool newNode
        )
    {
        _counters.NodeQueryCount++;

        var child = parent.ChildList.FirstOrDefault(c => _symbolComparer.Compare(c.Symbol, symbol, false));
        newNode = child is null;

        if (child is null)
        {
            var id = _symbolUidGenerator.Execute(symbol);
            child = new Node(id, symbol);

            parent.ChildList = AddNodeListItem(parent.ChildList, child);
            _counters.NodeCount++;
        }

        return child;
    }

    public void AddLink(Node source, Node target, SyntaxNode syntaxNode, LocationKind locationKind)
    {
        if (Links.Capacity == 0)
        {
            Links.Capacity = _counters.LinkedSymbolCount;
        }

        Links.Add(new Link()
        {
            Source = source,
            Target = target,
            Syntax = syntaxNode,
            LocationKind = locationKind,
        });

        _counters.LinkCount++;
    }

    public void AddLinkedSymbol(Node node, ISymbol symbol, SyntaxNode syntax, LocationKind locationKind)
    {
        _counters.LinkedSymbolQueryCount++;

        foreach (var currentItem in node.LinkedSymbolsList)
        {
            if (currentItem.Syntax.Span == syntax.Span
                && _symbolComparer.Compare(currentItem.Symbol, symbol, true)
                )
            {
                return;
            }
        }

        AddLinkedSymbol(node, new LinkedSymbol()
        {
            Symbol = symbol,
            Syntax = syntax,
            LocationKind = locationKind
        });
    }

    private void AddLinkedSymbol(Node node, LinkedSymbol linkedSymbol)
    {
        node.LinkedSymbolsList = AddNodeListItem(node.LinkedSymbolsList, linkedSymbol);
        _counters.LinkedSymbolCount++;
    }

    public void AddSyntaxLink(Node node, LocationKind locationKind, SyntaxNode syntax)
    {
        _counters.SyntaxLinkQueryCount++;
        foreach (var item in node.SyntaxLinkList)
        {
            if (item is NodeSyntaxLink nodeSyntaxLink && nodeSyntaxLink.IsSame(locationKind, syntax))
            {
                return;
            }
        }

        AddNodeSyntaxLink(node, new NodeSyntaxLink(locationKind, syntax));
    }

    public void AddAssemblySyntaxLink(Node node, string path)
    {
        _counters.SyntaxLinkQueryCount++;
        foreach (var item in node.SyntaxLinkList)
        {
            if (item is AssemblyNodeSyntaxLink assemblyNodeSyntaxLink && assemblyNodeSyntaxLink.IsSame(path))
            {
                return;
            }
        }

        AddNodeSyntaxLink(node, Utils.CreateAssemblySyntaxLink(path));
    }

    private void AddNodeSyntaxLink(Node node, INodeSyntaxLink syntaxLink)
    {
        node.SyntaxLinkList = AddNodeListItem(node.SyntaxLinkList, syntaxLink);
        _counters.SyntaxLinkCount++;
    }

    private static List<T> AddNodeListItem<T>(List<T> list, T item)
    {
        if (list.Count == 0)
        {
            list = new();
        }

        list.Add(item);
        return list;
    }
}