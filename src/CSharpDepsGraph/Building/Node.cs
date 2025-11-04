using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSharpDepsGraph.Building.Entities;
using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building;

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

    public void AddLinkedSymbol(LinkedSymbol item)
    {
        foreach (var linkedSymbol in LinkedSymbolsList)
        {
            if (linkedSymbol.SyntaxLink.Path == item.SyntaxLink.Path
                && linkedSymbol.SyntaxLink.Line == item.SyntaxLink.Line
                && linkedSymbol.SyntaxLink.Column == item.SyntaxLink.Column
                )
            {
                return;
            }
        }

        LinkedSymbolsList.Add(item);
    }

    public void AddLinkedSymbols(List<LinkedSymbol> items)
    {
        var uniqueFrom = items.Where(fromItem =>
        {
            var fromItemSyntaxLink = fromItem.SyntaxLink;

            foreach (var linkedSymbol in LinkedSymbolsList)
            {
                var toItemSyntaxLink = linkedSymbol.SyntaxLink;
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

        LinkedSymbolsList.AddRange(uniqueFrom);
    }
}