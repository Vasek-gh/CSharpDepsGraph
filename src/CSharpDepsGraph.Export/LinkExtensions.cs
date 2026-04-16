using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpDepsGraph.Export;

/// <summary>
/// Extensions for <see cref="ILink"/>
/// </summary>
public static class LinkExtensions
{
    /// <summary>
    /// Return link type for the link
    /// </summary>
    public static LinkType GetLinkType(this ILink link)
    {
        var syntax = link.SyntaxLink.Syntax;
        if (syntax is null)
        {
            return LinkType.Reference;
        }

        if (syntax is ObjectCreationExpressionSyntax)
        {
            return LinkType.Call;
        }

        var symbol = link.OriginalTarget.Symbol;

        if (symbol is IMethodSymbol && syntax.FirstAncestorOrSelf<InvocationExpressionSyntax>() != null)
        {
            return LinkType.Call;
        }

        if (symbol is ITypeSymbol typeSymbol && syntax.FirstAncestorOrSelf<SimpleBaseTypeSyntax>() != null)
        {
            var typeDeclNode = syntax.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            var baseTypeKind = typeSymbol.TypeKind;

            return typeDeclNode?.Keyword.IsKind(SyntaxKind.InterfaceKeyword) == true
                || baseTypeKind != TypeKind.Interface ?
                LinkType.Inherits
                : LinkType.Implements;
        }

        return LinkType.Reference;
    }
}