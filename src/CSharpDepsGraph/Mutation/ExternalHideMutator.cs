using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpDepsGraph.Mutation;

/// <summary>
/// todo
/// </summary>
public class ExternalHideMutator : IMutator
{
    private readonly bool _hideOnlyChilds;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalHideMutator"/> class.
    /// </summary>
    public ExternalHideMutator(bool hideOnlyChilds = true)
    {
        _hideOnlyChilds = hideOnlyChilds;
    }

    /// <inheritdoc/>
    public IGraph Run(IGraph graph)
    {
        var externalRootNode = graph.Root.Childs.Single(n => n.IsExternalsRoot());
        var externalsNodes = externalRootNode.CollectChildNodes().ToDictionary(n => n.Id);

        return new MutatedGraph()
        {
            Root = MutateRoot(graph.Root, externalRootNode),
            Links = MutateLinks(graph.Links, externalRootNode, externalsNodes)
        };
    }

    private INode MutateRoot(INode root, INode externalRootNode)
    {
        var childs = root.Childs.Where(c => c.Id != externalRootNode.Id);
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
            var externalIsSource = externalsNodes.ContainsKey(link.Source.Id);
            var externalIsTarget = externalsNodes.ContainsKey(link.Target.Id);

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