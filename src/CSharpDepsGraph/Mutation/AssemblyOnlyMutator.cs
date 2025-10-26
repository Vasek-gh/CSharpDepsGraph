using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Mutation;

/// <summary>
/// todo
/// </summary>
public class AssemblyOnlyMutator : IMutator
{
    private readonly Dictionary<string, INode> _newNodeIdMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyOnlyMutator"/> class.
    /// </summary>
    public AssemblyOnlyMutator()
    {
        _newNodeIdMap = new Dictionary<string, INode>();
    }

    /// <inheritdoc/>
    public IGraph Run(IGraph graph)
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
                _newNodeIdMap.Add(node.Id, assemblyNode);
                return true;
            });
        }

        return assemblyNodes.Select(n => MutatedNode.Copy(n, Array.Empty<INode>()));
    }

    private void CollectIds(INode node, INode parentNode)
    {
        _newNodeIdMap.Add(node.Id, parentNode);
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
            if (!_newNodeIdMap.TryGetValue(link.Source.Id, out var newSource)
                || !_newNodeIdMap.TryGetValue(link.Target.Id, out var newTarget)
                )
            {
                continue;
            }

            result.Add(MutatedLink.Copy(link, newSource, newTarget));
        }

        return result;
    }
}