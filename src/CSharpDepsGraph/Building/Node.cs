using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building;

[DebuggerDisplay("{Id}")]
internal class Node : INode
{
    public required string Id { get; set; }

    public required ISymbol? Symbol { get; set; }

    public IEnumerable<INode> Childs => ChildList;

    public required IEnumerable<SyntaxLink> SyntaxLinks { get; set; }

    public IEnumerable<LinkedSymbol> LinkedSymbols { get; set; } = Array.Empty<LinkedSymbol>();

    public List<Node> ChildList { get; } = new List<Node>();
}