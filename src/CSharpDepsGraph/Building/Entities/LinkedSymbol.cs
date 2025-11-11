using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building.Entities;

internal class LinkedSymbol
{
    public required ISymbol Symbol { get; init; }

    public required SyntaxNode Syntax { get; init; }

    public required LocationKind LocationKind { get; init; }
}