using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Mutation.Filtering;

public static class Filters
{
    public static DelegateFilter Empty { get; } = new DelegateFilter((node) => FilterAction.Skip);

    public static DelegateFilter HidePrivate { get; } = new DelegateFilter((node, parent) =>
    {
        var visible = node.Symbol == null
            || parent.Symbol is not ITypeSymbol
            || node.Symbol.IsTopLevelStatement()
            || node.Symbol.DeclaredAccessibility != Accessibility.Private;

        return visible ? FilterAction.Skip : FilterAction.Hide;
    });

    public static DelegateFilter HideMembers { get; } = new DelegateFilter((node, parent) =>
    {
        var visible = parent.Symbol is not ITypeSymbol
            || node.Symbol == null
            || (
                node.Symbol is ITypeSymbol typeSymbol
                && (
                    typeSymbol.TypeKind == TypeKind.Structure
                    || typeSymbol.TypeKind == TypeKind.Class
                    || typeSymbol.TypeKind == TypeKind.Interface
                    || typeSymbol.TypeKind == TypeKind.Delegate
                    || typeSymbol.TypeKind == TypeKind.Enum
                )
            );

        return visible ? FilterAction.Skip : FilterAction.Dissolve;
    });

    public static DelegateFilter HideTypes { get; } = new DelegateFilter((node) =>
    {
        var visible = node.Symbol == null
            || node.Symbol is IAssemblySymbol
            || node.Symbol is IModuleSymbol
            || node.Symbol is INamespaceSymbol;

        return visible ? FilterAction.Skip : FilterAction.Dissolve;
    });

    public static DelegateFilter HideNamespaces { get; } = new DelegateFilter((node) => // todo rename DissolveNamespaces
    {
        var visible = node.Symbol == null
            || node.Symbol is IAssemblySymbol
            || node.Symbol is IModuleSymbol;

        return visible ? FilterAction.Skip : FilterAction.Dissolve;
    });
}