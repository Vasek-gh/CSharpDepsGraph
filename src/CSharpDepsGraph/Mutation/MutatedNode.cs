using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Mutation;

[DebuggerDisplay("{Id}")]
internal class MutatedNode : INode
{
    public required string Id { get; init; }

    public required ISymbol? Symbol { get; init; }

    public required IEnumerable<INode> Childs { get; init; }

    public required IEnumerable<SyntaxLink> SyntaxLinks { get; set; }

    public static MutatedNode Copy(INode node, IEnumerable<INode> childs)
    {
        return new MutatedNode()
        {
            Id = node.Id,
            Symbol = node.Symbol,
            Childs = childs.ToArray(),
            SyntaxLinks = node.SyntaxLinks
        };
    }
}
