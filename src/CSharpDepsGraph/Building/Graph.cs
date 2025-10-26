using System.Collections.Generic;

namespace CSharpDepsGraph.Building;

internal class Graph : IGraph
{
    public required INode Root { get; init; }

    public required IEnumerable<ILink> Links { get; init; }
}