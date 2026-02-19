namespace CSharpDepsGraph.Transforming;

// todo c учетом что теперь нету из коробки корневого для внешних нужно проверять на признак внешнего через Utils.IsInMetadata
// еще остается вопрос что если проект не был передан в graphbuilder
/// <summary>
/// Removes all external nodes/assemblies
/// </summary>
public class ExternalHideTransformer : ITransformer
{
    private readonly bool _hideOnlyChilds;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalHideTransformer"/> class.
    /// </summary>
    public ExternalHideTransformer(bool hideOnlyChilds = true)
    {
        _hideOnlyChilds = hideOnlyChilds;
    }

    /// <inheritdoc/>
    public IGraph Execute(IGraph graph)
    {
        return graph;

        // todo
        /*var externalRootNode = graph.Root.Childs.Single(n => n.IsExternalsRoot());
        var externalsNodes = externalRootNode.CollectChildNodes().ToDictionary(n => n.Uid);

        return new MutatedGraph()
        {
            Root = MutateRoot(graph.Root, externalRootNode),
            Links = MutateLinks(graph.Links, externalRootNode, externalsNodes)
        };*/
    }

    private INode MutateRoot(INode root, INode externalRootNode)
    {
        var childs = root.Childs.Where(c => c.Uid != externalRootNode.Uid);
        if (_hideOnlyChilds)
        {
            childs = childs.Append(MutatedNode.Copy(externalRootNode, Array.Empty<INode>()));
        }

        return MutatedNode.Copy(
            root,
            childs
        );
    }

    private IEnumerable<ILink> MutateLinks(IEnumerable<ILink> links, INode externalRootNode, Dictionary<string, INode> externalsNodes)
    {
        var result = new List<ILink>();
        foreach (var link in links)
        {
            var externalIsSource = externalsNodes.ContainsKey(link.Source.Uid);
            var externalIsTarget = externalsNodes.ContainsKey(link.Target.Uid);

            if (!externalIsSource && !externalIsTarget)
            {
                result.Add(link);
                continue;
            }

            if (!_hideOnlyChilds)
            {
                continue;
            }

            if (externalIsSource)
            {
                result.Add(MutatedLink.Copy(link, externalRootNode, link.Target));
            }
            else
            {
                result.Add(MutatedLink.Copy(link, link.Source, externalRootNode));
            }
        }

        return result;
    }
}