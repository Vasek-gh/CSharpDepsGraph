using System.Collections.Generic;
using System.Linq;

namespace CSharpDepsGraph.Tests;

public static class GraphExtensions
{
    public static INode GetNode(this IGraph graph, string fullQualifiedName)
    {
        return graph.Root.GetNode(NodeExtensions.MakePath(AsmName.Test, fullQualifiedName));
    }

    public static INode GetNode(this IGraph graph, string assemblyName, string fullQualifiedName)
    {
        return graph.Root.GetNode(NodeExtensions.MakePath(assemblyName, fullQualifiedName));
    }

    public static bool HasLink(this IGraph graph, INode source, INode target)
    {
        return GetLinks(graph, source, target).Any();
    }

    public static IEnumerable<ILink> GetLinks(this IGraph graph, INode source, INode target)
    {
        return graph.Links.Where(l => l.Source.Id == source.Id && l.Target.Id == target.Id);
    }

    public static IEnumerable<ILink> GetOutgoingLinks(this IGraph graph, INode node)
    {
        return graph.Links.Where(l => l.Source.Id == node.Id);
    }

    public static IEnumerable<ILink> GetIncomingLinks(this IGraph graph, INode node)
    {
        return graph.Links.Where(l => l.Target.Id == node.Id);
    }
}