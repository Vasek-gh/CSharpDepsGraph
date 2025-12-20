namespace CSharpDepsGraph.Transforming.Filtering;

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
    /// The node will be deleted and all links leading to it or its children will be removed
    /// </summary>
    Hide,

    /// <summary>
    /// The node will be removed and all links leading to it or its children will be redirected to the parent node
    /// </summary>
    Dissolve,
}