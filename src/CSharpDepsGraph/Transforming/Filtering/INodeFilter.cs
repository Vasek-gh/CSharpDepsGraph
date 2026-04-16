namespace CSharpDepsGraph.Transforming.Filtering;

/// <summary>
/// Filter interface for nodes
/// </summary>
public interface INodeFilter
{
    /// <summary>
    /// Executes filter on node
    /// </summary>
    FilterAction Execute(NodeContext context);
}