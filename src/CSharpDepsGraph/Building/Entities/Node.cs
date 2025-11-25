using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building.Entities;

[DebuggerDisplay("{Id}")]
internal class Node : INode
{
    private List<INodeSyntaxLink> _syntaxLinkList;

    public string Id { get; }

    public ISymbol? Symbol { get; }

    public IEnumerable<INode> Childs => ChildList;

    public IEnumerable<INodeSyntaxLink> SyntaxLinks => _syntaxLinkList;

    public List<Node> ChildList { get; } = [];

    public required List<LinkedSymbol> LinkedSymbolsList { get; init; }

    public Node(string id, ISymbol? symbol, List<INodeSyntaxLink>? syntaxLinks = null)
    {
        Id = id;
        Symbol = symbol;

        _syntaxLinkList = syntaxLinks ?? Utils.GetEmptyList<INodeSyntaxLink>();
    }

    public void AddSyntaxLink(LocationKind locationKind, SyntaxNode syntax)
    {
        foreach (var item in _syntaxLinkList)
        {
            if (item is NodeSyntaxLink nodeSyntaxLink && nodeSyntaxLink.IsSame(locationKind, syntax))
            {
                return;
            }
        }

        AddNodeSyntaxLink(new NodeSyntaxLink(locationKind, syntax));
    }

    public void AddSyntaxReference(LocationKind locationKind, SyntaxReference syntaxReference)
    {
        foreach (var item in _syntaxLinkList)
        {
            if (item is NodeSyntaxLink nodeSyntaxLink && nodeSyntaxLink.IsSame(locationKind, syntaxReference))
            {
                return;
            }
        }

        AddNodeSyntaxLink(new NodeSyntaxLink(locationKind, syntaxReference));
    }

    public void AddAssemblySyntaxLink(string path)
    {
        foreach (var item in _syntaxLinkList)
        {
            if (item is AssemblyNodeSyntaxLink assemblyNodeSyntaxLink && assemblyNodeSyntaxLink.IsSame(path))
            {
                return;
            }
        }

        AddNodeSyntaxLink(Utils.CreateAssemblySyntaxLink(path));
    }

    private void AddNodeSyntaxLink(INodeSyntaxLink syntaxLink)
    {
        if (_syntaxLinkList.Count == 0)
        {
            _syntaxLinkList = new();
        }

        _syntaxLinkList.Add(syntaxLink);
    }
}