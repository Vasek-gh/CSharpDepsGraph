namespace CSharpDepsGraph.Transforming.Filtering;

/// <summary>
/// Filter node context
/// </summary>
public struct NodeContext
{
    /// <summary>
    /// Current node path
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// Parent of current node
    /// </summary>
    public required INode Parent { get; set; }

    /// <summary>
    /// Current node for which the filter is called
    /// </summary>
    public required INode Node { get; set; }
}
