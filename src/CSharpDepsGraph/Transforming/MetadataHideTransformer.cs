namespace CSharpDepsGraph.Transforming;

/// <summary>
/// Removes from the root all nodes that point to symbols from the metadata
/// </summary>
public class MetadataHideTransformer : ITransformer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataHideTransformer"/> class.
    /// </summary>
    public MetadataHideTransformer()
    {
    }

    /// <inheritdoc/>
    public IGraph Execute(IGraph graph)
    {
        return new MutatedGraph()
        {
            Root = MutateRoot(graph.Root),
            Links = MutateLinks(graph.Links)
        };
    }

    private static INode MutateRoot(INode root)
    {
        return MutatedNode.Copy(
            root,
            root.Childs.Where(c => !c.IsFromMetadata())
        );
    }

    private static IEnumerable<ILink> MutateLinks(IEnumerable<ILink> links)
    {
        return links.Where(l => !l.Source.IsFromMetadata() && !l.Target.IsFromMetadata());
    }
}