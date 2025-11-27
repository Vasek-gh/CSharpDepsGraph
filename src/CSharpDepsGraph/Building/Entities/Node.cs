using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building.Entities;

[DebuggerDisplay("{Id}")]
internal class Node : INode
{
    public string Id { get; }

    public ISymbol? Symbol { get; }

    public IEnumerable<INode> Childs => ChildList;

    public IEnumerable<INodeSyntaxLink> SyntaxLinks => SyntaxLinkList;

    public List<Node> ChildList { get; set; }

    public List<LinkedSymbol> LinkedSymbolsList { get; set; }

    public List<INodeSyntaxLink> SyntaxLinkList { get; set; }

    public Node(string id, ISymbol? symbol)
    {
        Id = id;
        Symbol = symbol;

        ChildList = Utils.GetEmptyList<Node>();
        SyntaxLinkList = Utils.GetEmptyList<INodeSyntaxLink>();
        LinkedSymbolsList = Utils.GetEmptyList<LinkedSymbol>();
    }

    public Node(string id, ISymbol? symbol, LinkedSymbol linkedSymbol)
    {
        Id = id;
        Symbol = symbol;

        ChildList = Utils.GetEmptyList<Node>();
        SyntaxLinkList = Utils.GetEmptyList<INodeSyntaxLink>();
        LinkedSymbolsList = [linkedSymbol];
    }

    public Node(string id, ISymbol? symbol, List<INodeSyntaxLink>? syntaxLinks = null)
    {
        Id = id;
        Symbol = symbol;

        ChildList = Utils.GetEmptyList<Node>();
        SyntaxLinkList = syntaxLinks ?? [];
        LinkedSymbolsList = Utils.GetEmptyList<LinkedSymbol>();
    }
}