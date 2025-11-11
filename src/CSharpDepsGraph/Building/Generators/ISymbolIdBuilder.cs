using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building;

/// <summary>
/// Creates a unique identifier for a symbol
/// </summary>
public interface ISymbolIdGenerator
{
    /// <summary>
    /// Creates an identifier
    /// </summary>
    string Execute(ISymbol symbol);

    /// <summary>
    /// Log internal statistic to logger
    /// </summary>
    void WriteStatistic();
}