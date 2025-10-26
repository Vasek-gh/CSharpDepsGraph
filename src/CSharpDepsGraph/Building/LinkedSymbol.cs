using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building;

internal class LinkedSymbol
{
    public required ISymbol Symbol { get; init; }

    public required SyntaxLink SyntaxLink { get; init; }
}