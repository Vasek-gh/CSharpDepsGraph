using System.Collections.Generic;

namespace CSharpDepsGraph.Mutation;

internal class MutatedGraph : IGraph
{
    public required INode Root { get; init; }

    public required IEnumerable<ILink> Links { get; init; }
}