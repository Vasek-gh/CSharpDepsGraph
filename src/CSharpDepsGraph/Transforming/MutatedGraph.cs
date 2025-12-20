namespace CSharpDepsGraph.Transforming;

internal class MutatedGraph : IGraph
{
    public required INode Root { get; init; }

    public required IEnumerable<ILink> Links { get; init; }
}