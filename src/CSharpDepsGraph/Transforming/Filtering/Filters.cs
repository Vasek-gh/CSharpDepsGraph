using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Transforming.Filtering;

/// <summary>
/// A set of ready-made filters
/// </summary>
public static class Filters
{
    /// <summary>
    /// A stub filter instance. This filter does nothing
    /// </summary>
    public static DelegateFilter Empty { get; } = new DelegateFilter((node) => FilterAction.Skip);

    /// <summary>
    /// This filter can use to hide all private members
    /// </summary>
    public static DelegateFilter HidePrivate { get; } = new DelegateFilter((node, parent) =>
    {
        var visible = node.Symbol == null
            || parent.Symbol is not ITypeSymbol
            || node.Symbol.IsTopLevelStatement()
            || node.Symbol.DeclaredAccessibility != Accessibility.Private;

        return visible ? FilterAction.Skip : FilterAction.Hide;
    });

    /// <summary>
    /// This filter can use to dissolve all members
    /// </summary>
    public static DelegateFilter DissolveMembers { get; } = new DelegateFilter((node, parent) =>
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

    /// <summary>
    /// This filter can use to dissolve all types
    /// </summary>
    public static DelegateFilter DissolveTypes { get; } = new DelegateFilter((node) =>
    {
        // todo ITypeSymbol
        var visible = node.Symbol == null
            || node.Symbol is IAssemblySymbol
            || node.Symbol is IModuleSymbol
            || node.Symbol is INamespaceSymbol;

        return visible ? FilterAction.Skip : FilterAction.Dissolve;
    });

    /// <summary>
    /// This filter can use to dissolve all namespaces
    /// </summary>
    public static DelegateFilter DissolveNamespaces { get; } = new DelegateFilter((node) =>
    {
        // todo INamespaceSymbol
        var visible = node.Symbol == null
            || node.Symbol is IAssemblySymbol
            || node.Symbol is IModuleSymbol;

        return visible ? FilterAction.Skip : FilterAction.Dissolve;
    });
}