namespace CSharpDepsGraph.Mutation.Filtering;

/// <summary>
/// Determines what action the filter needs to perform with the node
/// </summary>
public enum FilterAction
{
    /// <summary>
    /// When a node is skipped, the node remains as is
    /// </summary>
    Skip,

    /// <summary>
    /// When a node dissolves, the node is hidden and its links are linked to its parent
    /// </summary>
    Hide,

    /// <summary>
    /// When a node is hidden, it is deleted along with all its connections
    /// </summary>
    Dissolve,
}