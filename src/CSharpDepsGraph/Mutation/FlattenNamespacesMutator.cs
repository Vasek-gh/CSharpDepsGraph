using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Mutation;

/// <summary>
/// todo
/// </summary>
public class FlattenNamespacesMutator : IMutator
{
    /// <inheritdoc/>
    public IGraph Run(IGraph graph)
    {
        var root = DoRun(graph.Root);

        return new MutatedGraph()
        {
            Root = root,
            Links = graph.Links
        };
    }

    private static INode DoRun(INode node)
    {
        var childs = new List<INode>();
        foreach (var child in node.Childs)
        {
            if (IsNodeCanContainsNamespace(child))
            {
                childs.Add(DoRun(child));
                continue;
            }

            if (child.Symbol is not INamespaceSymbol)
            {
                childs.Add(child);
                continue;
            }

            foreach (var namespaceNode in CollectNamespaces(child))
            {
                AppendNamespace(namespaceNode, childs);
            }
        }

        return MutatedNode.Copy(node, childs);
    }

    private static bool IsNodeCanContainsNamespace(INode node)
    {
        return node.Symbol == null || node.Symbol is IAssemblySymbol || node.Symbol is IModuleSymbol;
    }

    private static void AppendNamespace(INode node, List<INode> namespaceNodes)
    {
        if (node.Symbol is not INamespaceSymbol)
        {
            return;
        }

        var newChilds = node.Childs.Where(c => c.Symbol is not INamespaceSymbol);
        if (newChilds.Any())
        {
            var newNode = MutatedNode.Copy(node, newChilds);
            namespaceNodes.Add(newNode);
        }
    }

    private static List<INode> CollectNamespaces(INode node)
    {
        var result = new List<INode>();
        node.VisitNodes((node) =>
        {
            if (node.Symbol is not INamespaceSymbol)
            {
                return false;
            }

            result.Add(node);
            return true;
        });

        return result;
    }
}