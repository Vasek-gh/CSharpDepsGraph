using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph;

/// <summary>
/// Extension methods for the <see cref="ISymbol"/>
/// </summary>
public static class SymbolExtensions
{
    /// <summary>
    /// Determines if the symbol represents the global namespace
    /// </summary>
    public static bool IsGlobalNamespace(this ISymbol symbol)
    {
        return symbol is INamespaceSymbol namespaceSymbol && namespaceSymbol.IsGlobalNamespace;
    }

    /// <summary>
    /// Determines if the symbol represents the top level statement
    /// </summary>
    public static bool IsTopLevelStatement(this ISymbol? symbol)
    {
        return symbol != null && symbol.Kind == SymbolKind.Method && symbol.Name == "<Main>$";
    }
}
