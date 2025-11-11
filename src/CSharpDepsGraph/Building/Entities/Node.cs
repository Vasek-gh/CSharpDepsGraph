using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

    public void AddSyntaxReference(LocationKind locationKind, SyntaxReference syntaxReference)
    {
        var path = syntaxReference.SyntaxTree.FilePath;
        var span = syntaxReference.Span;

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

    public void AddLinkedSymbols(List<LinkedSymbol> items)
    {
        var uniqueFrom = items.Where(fromItem =>
        {
            var fromItemSyntax = fromItem.Syntax;

            foreach (var linkedSymbol in LinkedSymbolsList)
            {
                var toItemSyntax = linkedSymbol.Syntax;

                if (toItemSyntax.SyntaxTree.FilePath == fromItemSyntax.SyntaxTree.FilePath
                    && toItemSyntax.Span == fromItemSyntax.Span
                    && linkedSymbol.LocationKind == fromItem.LocationKind
                    )
                {
                    return false;
                }
            }

            return true;
        });

        LinkedSymbolsList.AddRange(uniqueFrom);
    }
}