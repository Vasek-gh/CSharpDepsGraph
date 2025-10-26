using System.Collections.Generic;
using System.Linq;

namespace CSharpDepsGraph.Mutation.Filtering;

// todo надо проверить что происходит с линками котрые присоедены к дечерней ноде которая удаляется
/// <summary>
/// todo
/// </summary>
public class FilterMutator : IMutator
{
    private readonly IEnumerable<IFilter> _filters;
    private readonly Dictionary<string, INode> _nodeMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterMutator"/> class.
    /// </summary>
    public FilterMutator(params IFilter[] filters)
        : this(filters as IEnumerable<IFilter>)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterMutator"/> class.
    /// </summary>
    public FilterMutator(IEnumerable<IFilter> filters)
    {
        _filters = filters;
        _nodeMap = new Dictionary<string, INode>();
    }

    /// <inheritdoc/>
    public IGraph Run(IGraph graph)
    {
        var root = MutateNode(graph.Root);
        var links = MutateLinks(graph.Links);

        return new MutatedGraph()
        {
            Root = root,
            Links = links
        };
    }

    private INode MutateNode(INode node)
    {
        var result = node;
        foreach (var inner in node.CollectChildNodes())
        {
            _nodeMap[inner.Id] = inner;
        }

        if (node.Childs.Any(child => child.Childs.Any() || GetFilterAction(node, child) != FilterAction.Skip))
        {
            result = MutatedNode.Copy(node, CollectChilds(node));
        }

        _nodeMap[result.Id] = result;

        return result;
    }

    private List<INode> CollectChilds(INode node)
    {
        var result = new List<INode>();
        foreach (var child in node.Childs)
        {
            var filterAction = GetFilterAction(node, child);

            if (filterAction == FilterAction.Hide)
            {
                foreach (var filteredNode in child.CollectChildNodes())
                {
                    _nodeMap.Remove(filteredNode.Id);
                }

                continue;
            }

            if (filterAction == FilterAction.Dissolve)
            {
                foreach (var filteredNode in child.CollectChildNodes())
                {
                    _nodeMap[filteredNode.Id] = node;
                }

                continue;
            }

            var mutatedNode = MutateNode(child);
            result.Add(mutatedNode);
        }

        return result;
    }

    private FilterAction GetFilterAction(INode parent, INode node)
    {
        foreach (var filter in _filters)
        {
            var action = filter.Execute(parent, node);
            if (action != FilterAction.Skip)
            {
                return action;
            }
        }

        return FilterAction.Skip;
    }

    private List<ILink> MutateLinks(IEnumerable<ILink> links)
    {
        var result = new List<ILink>();
        foreach (var link in links)
        {
            var sourceNode = GetNode(link.Source);
            var targetNode = GetNode(link.Target);

            if (sourceNode == null || targetNode == null)
            {
                continue;
            }

            if (sourceNode.Id != link.Source.Id || targetNode.Id != link.Target.Id)
            {
                result.Add(MutatedLink.Copy(link, sourceNode, targetNode));
                continue;
            }

            result.Add(link);
        }

        return result;
    }

    private INode? GetNode(INode node)
    {
        _nodeMap.TryGetValue(node.Id, out var result);

        return result;
    }
}