namespace CSharpDepsGraph.Mutation.Filtering;

/// <summary>
/// Filter interface for nodes
/// </summary>
public interface IFilter
{
    /// <summary>
    /// Executes filter on node
    /// </summary>
    FilterAction Execute(INode parent, INode node);
}