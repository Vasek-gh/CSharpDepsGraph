using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Transforming;

[DebuggerDisplay("{Uid}")]
internal class MutatedNode : INode
{
    public required string Uid { get; init; }

    public required ISymbol? Symbol { get; init; }

    public required IEnumerable<INode> Childs { get; init; }

    public required IEnumerable<INodeSyntaxLink> SyntaxLinks { get; set; }

    public static MutatedNode Copy(INode node, IEnumerable<INode> childs)
    {
        return new MutatedNode()
        {
            Uid = node.Uid,
            Symbol = node.Symbol,
            Childs = childs,
            SyntaxLinks = node.SyntaxLinks
        };
    }
}
