using System.Collections.Generic;

namespace CSharpDepsGraph.Building.Entities;

internal class Graph : IGraph
{
    public required INode Root { get; init; }

    public required IEnumerable<ILink> Links { get; init; }
}