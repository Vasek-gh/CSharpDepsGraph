using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building.Generators;

/// <summary>
/// Creates a unique identifier for a symbol
/// </summary>
public interface ISymbolUidGenerator // todo internal
{
    /// <summary>
    /// Creates an identifier
    /// </summary>
    string Execute(ISymbol symbol);
}