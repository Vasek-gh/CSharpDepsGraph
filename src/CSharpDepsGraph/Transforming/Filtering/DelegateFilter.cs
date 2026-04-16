namespace CSharpDepsGraph.Transforming.Filtering;

/// <summary>
/// Implementation of the <see cref="INodeFilter"/> interface that delegates filtering logic to an external delegate
/// </summary>
public class DelegateFilter : INodeFilter
{
    private readonly Func<INode, INode, FilterAction> _action;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateFilter"/> class.
    /// </summary>
    public DelegateFilter(Func<INode, FilterAction> action)
    {
        _action = (p, n) => action(n);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateFilter"/> class.
    /// </summary>
    public DelegateFilter(Func<INode, INode, FilterAction> action)
    {
        _action = action;
    }

    /// <inheritdoc/>
    public FilterAction Execute(NodeContext context)
    {
        return _action(context.Parent, context.Node);
    }
}