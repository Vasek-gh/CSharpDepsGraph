using System.Collections.Generic;

namespace CSharpDepsGraph;

/// <summary>
/// Dependency graph
/// </summary>
public interface IGraph
{
    /// <summary>
    /// Virtual root node
    /// </summary>
    INode Root { get; }

    /// <summary>
    /// List of edges (dependencies) of the graph
    /// </summary>
    IEnumerable<ILink> Links { get; }
}