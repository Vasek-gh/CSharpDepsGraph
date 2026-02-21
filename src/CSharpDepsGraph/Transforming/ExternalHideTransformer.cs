namespace CSharpDepsGraph.Transforming;

// todo c учетом что теперь нету из коробки корневого для внешних нужно проверять на признак внешнего через Utils.IsInMetadata
// еще остается вопрос что если проект не был передан в graphbuilder
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

    private INode MutateRoot(INode root)
    {
        return MutatedNode.Copy(
            root,
            root.Childs.Where(c => !c.IsFromMetadata())
        );
    }

    private IEnumerable<ILink> MutateLinks(IEnumerable<ILink> links)
    {
        return links.Where(l => !l.Source.IsFromMetadata() && !l.Target.IsFromMetadata());
    }
}