using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building;

[DebuggerDisplay("{Id}")]
internal class Node : INode
{
    public required string Id { get; set; }

    public required ISymbol? Symbol { get; set; }

    public IEnumerable<INode> Childs => ChildList;

    public IEnumerable<SyntaxLink> SyntaxLinks => SyntaxLinkList;

    public List<Node> ChildList { get; } = [];

    public required List<SyntaxLink> SyntaxLinkList { get; init; }

    public required List<LinkedSymbol> LinkedSymbolsList { get; init; }

    public void AddSyntaxLinks(IEnumerable<SyntaxLink> items)
    {
        if (SyntaxLinkList.Count > 0)
        {
            return;
        }

        SyntaxLinkList.AddRange(items);
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