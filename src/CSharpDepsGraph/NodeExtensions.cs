using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph;

/// <summary>
/// Extension methods for the <see cref="INode"/>
/// </summary>
public static class NodeExtensions
{
    /// <summary>
    /// Determines if the node is root
    /// </summary>
    public static bool IsRoot(this INode node)
    {
        return node.Id == GraphConsts.RootNodeId;
    }

    /// <summary>
    /// Determines if the node is root for external nodes
    /// </summary>
    public static bool IsExternalsRoot(this INode node)
    {
        return node.Id == GraphConsts.ExternalRootNodeId;
    }

    /// <summary>
    /// Determines if the node is external
    /// </summary>
    public static bool IsExternal(this INode node)
    {
        return node.SyntaxLinks.All(sl => sl.FileKind == SyntaxFileKind.External);
    }

    /// <summary>
    /// Determines if the node created for generated code
    /// </summary>
    public static bool IsGenerated(this INode node)
    {
        return node.SyntaxLinks.All(sl => sl.FileKind == SyntaxFileKind.Generated);
    }

    /// <summary>
    /// Collecting child nodes based on a predicate
    /// </summary>
    public static IEnumerable<INode> CollectChildNodes(this INode node, Func<INode, bool>? predicate = null)
    {
        var result = new List<INode>();
        VisitNodes(node, (child) =>
        {
            result.Add(child);
            return predicate?.Invoke(child) ?? true;
        });

        return result;
    }

    /// <summary>
    /// Collecting child nodes data based on a predicate
    /// </summary>
    public static IEnumerable<T> CollectNodeData<T>(this INode node, Func<INode, T> action)
    {
        var result = new List<T>();
        VisitNodes(node, (node) =>
        {
            result.Add(action(node));
            return true;
        });

        return result;
    }

    /// <summary>
    /// Visits all nodes down the hierarchy. The predicate is used to determine what to stop
    /// </summary>
    public static void VisitNodes(this INode node, Func<INode, bool> action)
    {
        if (!action(node))
        {
            return;
        }

        foreach (var child in node.Childs)
        {
            VisitNodes(child, action);
        }
    }

    /// <summary>
    /// Visits all nodes down the hierarchy. The predicate is used to determine what to stop
    /// </summary>
    public static void VisitNodes(this INode node, INode? parent, Func<INode, INode?, bool> action)
    {
        if (!action(node, parent))
        {
            return;
        }

        foreach (var child in node.Childs)
        {
            VisitNodes(child, node, action);
        }
    }
}