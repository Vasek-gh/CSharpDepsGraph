using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Export;

/// <summary>
/// Extensions for <see cref="ISymbol"/>
/// </summary>
public static class SymbolExtensions
{
    /// <summary>
    /// Return node type for the symbol
    /// </summary>
    public static NodeType? GetNodeType(this ISymbol symbol)
    {
        return symbol switch
        {
            _ when symbol is IAssemblySymbol => NodeType.Assembly,
            _ when symbol is INamespaceSymbol => NodeType.Namespace,
            _ when symbol is INamedTypeSymbol typeSymbol && typeSymbol.IsRecord => NodeType.Record,
            _ when symbol is INamedTypeSymbol typeSymbol && typeSymbol.TypeKind == TypeKind.Enum => NodeType.Enum,
            _ when symbol is INamedTypeSymbol typeSymbol && typeSymbol.TypeKind == TypeKind.Class => NodeType.Class,
            _ when symbol is INamedTypeSymbol typeSymbol && typeSymbol.TypeKind == TypeKind.Structure => NodeType.Structure,
            _ when symbol is INamedTypeSymbol typeSymbol && typeSymbol.TypeKind == TypeKind.Interface => NodeType.Interface,
            _ when symbol is INamedTypeSymbol typeSymbol && typeSymbol.TypeKind == TypeKind.Delegate => NodeType.Delegate,
            _ when symbol is IFieldSymbol fieldSymbol && fieldSymbol.IsConst => NodeType.Const,
            _ when symbol is IFieldSymbol => NodeType.Field,
            _ when symbol is IEventSymbol => NodeType.Event,
            _ when symbol is IPropertySymbol => NodeType.Property,
            _ when symbol is IMethodSymbol => NodeType.Method,
            _ => null
        };
    }

    /// <summary>
    /// Return caption for the symbol
    /// </summary>
    public static string? GetCaption(this ISymbol? symbol)
    {
        if (symbol == null)
        {
            return null;
        }

        if (symbol.IsTopLevelStatement())
        {
            return null;
        }

        if (symbol is IAssemblySymbol)
        {
            return $"{symbol.Name}.dll";
        }

        if (symbol is INamespaceSymbol namespaceSymbol)
        {

            return namespaceSymbol.IsGlobalNamespace
                ? $"global::{symbol.ContainingSymbol.Name}"
                : symbol.ToDisplayString();
        }

        return symbol.ToDisplayString(SymbolDisplayFormat);
    }

    private static SymbolDisplayFormat SymbolDisplayFormat { get; } =
        new SymbolDisplayFormat(
            globalNamespaceStyle:
                SymbolDisplayGlobalNamespaceStyle.Omitted,
            genericsOptions:
                SymbolDisplayGenericsOptions.IncludeTypeParameters,
            memberOptions:
                SymbolDisplayMemberOptions.IncludeParameters |
                SymbolDisplayMemberOptions.IncludeRef,
            kindOptions:
                SymbolDisplayKindOptions.None,
            parameterOptions:
                SymbolDisplayParameterOptions.IncludeName,
            localOptions: SymbolDisplayLocalOptions.IncludeType,
            miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
            );
}