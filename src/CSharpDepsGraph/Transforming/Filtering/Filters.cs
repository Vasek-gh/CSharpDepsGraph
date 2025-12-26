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
    public static DelegateFilter HidePrivate { get; } = new DelegateFilter((parent, node) =>
    {
        var visible = node.Symbol?.DeclaredAccessibility != Accessibility.Private;

        return visible ? FilterAction.Skip : FilterAction.Hide;
    });

    /// <summary>
    /// This filter can use to dissolve all members
    /// </summary>
    public static DelegateFilter DissolveMembers { get; } = new DelegateFilter((parent, node) =>
    {
        var visible = parent.Symbol is not ITypeSymbol
            || node.Symbol is ITypeSymbol typeSymbol;

        return visible ? FilterAction.Skip : FilterAction.Dissolve;
    });

    /// <summary>
    /// This filter can use to dissolve all types
    /// </summary>
    public static DelegateFilter DissolveTypes { get; } = new DelegateFilter((node) =>
    {
        var visible = node.Symbol is not ITypeSymbol;

        return visible ? FilterAction.Skip : FilterAction.Dissolve;
    });

    /// <summary>
    /// This filter can use to dissolve all namespaces
    /// </summary>
    public static DelegateFilter DissolveNamespaces { get; } = new DelegateFilter((node) =>
    {
        var visible = node.Symbol is not INamespaceSymbol;

        return visible ? FilterAction.Skip : FilterAction.Dissolve;
    });
}