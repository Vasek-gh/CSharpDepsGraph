using System;

namespace CSharpDepsGraph.Mutation.Filtering;

public class DelegateFilter : IFilter
{
    private readonly Func<INode, INode, FilterAction> _action;

    public DelegateFilter(Func<INode, FilterAction> action)
    {
        _action = (p, n) => action(n);
    }

    public DelegateFilter(Func<INode, INode, FilterAction> action)
    {
        _action = action;
    }

    /// <inheritdoc/>
    public FilterAction Execute(INode parent, INode node)
    {
        return _action(parent, node);
    }
}