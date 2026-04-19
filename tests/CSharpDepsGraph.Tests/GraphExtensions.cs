namespace CSharpDepsGraph.Tests;

public static class GraphExtensions
{
    public static INode GetNode(this IGraph graph, string symbolPath)
    {
        return graph.GetNode(AsmName.Test, symbolPath);
    }

    public static INode GetNode(this IGraph graph, string assemblyName, string? symbolPath)
    {
        return graph.Root.GetNode(MakeFullPath(assemblyName, symbolPath));
    }

    public static INode[] GetNodes(this IGraph graph, string assemblyName, string? symbolPath)
    {
        return graph.Root.GetNodes(MakeFullPath(assemblyName, symbolPath));
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

    public static ILink[] GetLinks(this IGraph graph, INode source, INode target)
    {
        return graph.Links.Where(l => l.Source.Uid == source.Uid && l.Target.Uid == target.Uid).ToArray();
    }

    public static ILink[] GetOutgoingLinks(this IGraph graph, INode node)
    {
        return graph.Links.Where(l => l.Source.Uid == node.Uid).ToArray();
    }

    public static ILink[] GetIncomingLinks(this IGraph graph, INode node)
    {
        return graph.Links.Where(l => l.Target.Uid == node.Uid).ToArray();
    }
}