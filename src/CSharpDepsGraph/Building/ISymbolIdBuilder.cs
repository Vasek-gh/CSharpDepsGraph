using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building;

/// <summary>
/// Creates a unique identifier for a symbol
/// </summary>
public interface ISymbolIdBuilder
{
    /// <summary>
    /// Creates an identifier
    /// </summary>
    string Execute(ISymbol symbol);
}