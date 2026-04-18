namespace CSharpDepsGraph.Transforming.Filtering;


/// <summary>
/// This transformer can be used to filter graph nodes
/// </summary>
public class FilterTransformer : ITransformer
{
    private readonly IEnumerable<INodeFilter> _filters;
    private readonly Dictionary<string, INode> _nodeMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterTransformer"/> class.
    /// </summary>
    public FilterTransformer(params INodeFilter[] filters)
        : this(filters as IEnumerable<INodeFilter>)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterTransformer"/> class.
    /// </summary>
    public FilterTransformer(IEnumerable<INodeFilter> filters)
    {
        _filters = filters.ToArray();
        _nodeMap = new();
    }

    /// <inheritdoc/>
    public IGraph Execute(IGraph graph)
    {
        if (!_filters.Any())
        {
            return graph;
        }

        var rootContext = new NodeContext()
        {
            Node = graph.Root,
            Parent = graph.Root, // Because MutateNode only processes child elements, it doesn't matter what's in that field.
            Path = ""
        };

        var root = MutateNode(rootContext);
        var links = MutateLinks(graph.Links);

        return new MutatedGraph()
        {
            Root = root,
            Links = links
        };
    }

    private INode MutateNode(NodeContext nodeContext)
    {
        _nodeMap[nodeContext.Node.Uid] = nodeContext.Node;

        var childs = new List<INode>();
        var childsChanged = false;

        foreach (var child in nodeContext.Node.Childs)
        {
            var childContext = MakeChildContext(nodeContext, child);

            var action = CheckNode(nodeContext.Node, childContext);
            if (action != FilterAction.Skip)
            {
                childsChanged = true;
                continue;
            }

            var newChild = MutateNode(childContext);
            if (!ReferenceEquals(child, newChild))
            {
                childsChanged = true;
            }

            childs.Add(newChild);
        }

        if (!childsChanged)
        {
            return nodeContext.Node;
        }

        return MutatedNode.Copy(nodeContext.Node, childs);
    }

    private FilterAction CheckNode(INode parent, NodeContext nodeContext)
    {
        var action = CheckNode(nodeContext);
        if (action == FilterAction.Dissolve)
        {
            nodeContext.Node.VisitNodes(n =>
            {
                _nodeMap[n.Uid] = parent;
                return true;
            });
        }

        return action;
    }

    private FilterAction CheckNode(NodeContext context)
    {
        foreach (var filter in _filters)
        {
            var action = filter.Execute(context);
            if (action != FilterAction.Skip)
            {
                return action;
            }
        }

        return FilterAction.Skip;
    }

    private static NodeContext MakeChildContext(NodeContext parentContext, INode node)
    {
        var childPath = (node.Symbol?.Name ?? node.Uid);
        return new NodeContext()
        {
            Path = string.IsNullOrEmpty(parentContext.Path)
                ? childPath
                : parentContext.Path + "/" + childPath,
            Parent = parentContext.Node,
            Node = node
        };
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

            if (sourceNode.Uid != link.Source.Uid || targetNode.Uid != link.Target.Uid)
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
        _nodeMap.TryGetValue(node.Uid, out var result);

        return result;
    }
}