using System.Collections.Generic;
using System.Linq;

namespace CSharpDepsGraph.Tests;

public static class GraphExtensions
{
    public static INode GetNode(this IGraph graph, string symbolPath)
    {
        return graph.GetNode(AsmName.Test, symbolPath);
    }

    public static INode GetNode(this IGraph graph, string assemblyName, string symbolPath)
    {
        return graph.Root.GetNode(MakeFullPath(assemblyName, symbolPath));
    }

    public static INode GetRootNode(this IGraph graph, string fullSymbolPath)
    {
        return graph.Root.GetNode(fullSymbolPath);
    }

    public static INode[] GetRootNodes(this IGraph graph, string fullSymbolPath)
    {
        return graph.Root.GetNodes(fullSymbolPath);
    }

    private static string MakeFullPath(string assemblyName, string? symbolPath)
    {
        return symbolPath == null
            ? assemblyName
            : $"{assemblyName}/{symbolPath}";
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