using System.Collections.Generic;
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

    /// <summary>
    /// Returns all syntax references for the symbol
    /// </summary>
    public static IEnumerable<SyntaxReference> GetSyntaxReference(this ISymbol symbol)
    {
        if (symbol is IMethodSymbol methodSymbol)
        {
            var result = new List<SyntaxReference>();
            result.AddRange(methodSymbol.DeclaringSyntaxReferences);

            if (methodSymbol.PartialDefinitionPart != null)
            {
                result.AddRange(methodSymbol.PartialDefinitionPart.DeclaringSyntaxReferences);
            }

            if (methodSymbol.PartialImplementationPart != null)
            {
                result.AddRange(methodSymbol.PartialImplementationPart.DeclaringSyntaxReferences);
            }

            return result;
        }

        return symbol.DeclaringSyntaxReferences;
    }
}
