using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Transforming;

/// <summary>
/// Leaves only assembly nodes, preserving the relationships between them
/// </summary>
public class AssemblyOnlyTransformer : ITransformer
{
    private readonly Dictionary<string, INode> _newNodeIdMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyOnlyTransformer"/> class.
    /// </summary>
    public AssemblyOnlyTransformer()
    {
        _newNodeIdMap = new Dictionary<string, INode>();
    }

    /// <inheritdoc/>
    public IGraph Execute(IGraph graph)
    {
        var rootNodes = GetAssemblies(graph.Root);

        var externalNode = graph.Root.Childs.SingleOrDefault(c => c.IsExternalsRoot());
        if (externalNode != null)
        {
            rootNodes = rootNodes.Concat(new[] {
                MutatedNode.Copy(externalNode, GetAssemblies(externalNode))
            });
        }

        return new MutatedGraph()
        {
            Root = MutatedNode.Copy(graph.Root, rootNodes),
            Links = MutateLinks(graph.Links)
        };
    }

    private IEnumerable<INode> GetAssemblies(INode assemblyNodesRoot)
    {
        var assemblyNodes = assemblyNodesRoot.Childs.Where(n => n.Symbol is IAssemblySymbol);

        foreach (var assemblyNode in assemblyNodes)
        {
            assemblyNode.VisitNodes(node =>
            {
                _newNodeIdMap.Add(node.Uid, assemblyNode);
                return true;
            });
        }

        return assemblyNodes.Select(n => MutatedNode.Copy(n, Array.Empty<INode>()));
    }

    private void CollectIds(INode node, INode parentNode)
    {
        _newNodeIdMap.Add(node.Uid, parentNode);
        foreach (var child in node.Childs)
        {
            CollectIds(child, parentNode);
        }
    }

    private IEnumerable<ILink> MutateLinks(IEnumerable<ILink> links)
    {
        var result = new List<ILink>();
        foreach (var link in links)
        {
            if (!_newNodeIdMap.TryGetValue(link.Source.Uid, out var newSource)
                || !_newNodeIdMap.TryGetValue(link.Target.Uid, out var newTarget)
                )
            {
                continue;
            }

            result.Add(MutatedLink.Copy(link, newSource, newTarget));
        }

        return result;
    }
}