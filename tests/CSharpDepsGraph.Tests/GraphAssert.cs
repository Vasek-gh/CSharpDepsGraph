using NUnit.Framework;
using System.Linq;

namespace CSharpDepsGraph.Tests;

internal static class GraphAssert
{
    public static void HasSymbol(IGraph graph, string fullQualifiedName)
    {
        HasSymbol(graph, (AsmName.Test, fullQualifiedName));
    }

    public static void HasSymbol(IGraph graph, params (string assemblyName, string? fullQualifiedName)[] paths)
    {
        foreach (var item in paths)
        {
            graph.GetNode(item.assemblyName, item.fullQualifiedName);
        }
    }

    public static void HasNotSymbol(IGraph graph, string fullQualifiedName)
    {
        HasNotSymbol(graph, (AsmName.Test, fullQualifiedName));
    }

    public static void HasNotSymbol(IGraph graph, params (string assemblyName, string? fullQualifiedName)[] paths)
    {
        foreach (var item in paths)
        {
            INode? child = null;
            try
            {
                child = graph.GetNode(item.assemblyName, item.fullQualifiedName);
            }
            catch (AssertionException)
            {
                continue;
            }

            if (child != null)
            {
                throw new AssertionException($"Unexpected symbol {child.Uid}");
            }
        }
    }

    public static void HasLink(
        IGraph graph,
        string testAsmFullQualifiedName,
        params (string assemblyName, string fullQualifiedName)[] targets
        )
    {
        CheckLinks(graph, false, (AsmName.Test, testAsmFullQualifiedName), targets);
    }

    public static void HasLink(
        IGraph graph,
        (string assemblyName, string fullQualifiedName) source,
        params (string assemblyName, string fullQualifiedName)[] targets
        )
    {
        CheckLinks(graph, false, source, targets);
    }

    public static void HasExactLink(
        IGraph graph,
        string testAsmFullQualifiedName,
        params (string assemblyName, string fullQualifiedName)[] targets
        )
    {
        CheckLinks(graph, true, (AsmName.Test, testAsmFullQualifiedName), targets);
    }

    public static void HasExactLink(
        IGraph graph,
        (string assemblyName, string fullQualifiedName) source,
        params (string assemblyName, string fullQualifiedName)[] targets
        )
    {
        CheckLinks(graph, true, source, targets);
    }

    private static void CheckLinks(
        IGraph graph,
        bool exact,
        (string assemblyName, string fullQualifiedName) source,
        params (string assemblyName, string fullQualifiedName)[] targets
        )
    {
        var sourceNode = graph.GetNode(source.assemblyName, source.fullQualifiedName);
        var targetNodes = targets.Select(t => graph.GetNode(t.assemblyName, t.fullQualifiedName))
            .ToDictionary(i => i.Uid) // check duplicates
            .Select(kv => kv.Value);

        var outgoingLinks = graph.GetOutgoingLinks(sourceNode)
            .GroupBy(n => n.Target.Uid)
            .Select(g => g.First())
            .ToArray();

        if (exact && outgoingLinks.Length != targets.Length)
        {
            throw new AssertionException($"The node({sourceNode.Uid}) has unexpected links");
        }

        foreach (var targetNode in targetNodes)
        {
            var link = outgoingLinks.SingleOrDefault(l => l.Target.Uid == targetNode.Uid)
                ?? throw new AssertionException($"{sourceNode.Uid} has not link to {targetNode.Uid}");
        }
    }

    public static void HasNotLink(
        IGraph graph,
        string testAsmFullQualifiedName,
        params (string assemblyName, string fullQualifiedName)[] target
        )
    {
        foreach (var item in target)
        {
            HasNotLink(graph,
                (AsmName.Test, testAsmFullQualifiedName),
                (item.assemblyName, item.fullQualifiedName)
            );
        }
    }

    public static void HasNotLink(
        IGraph graph,
        (string assemblyName, string fullQualifiedName) source,
        (string assemblyName, string fullQualifiedName) target
        )
    {
        var sourceNode = graph.GetNode(source.assemblyName, source.fullQualifiedName);
        var targetNode = graph.GetNode(target.assemblyName, target.fullQualifiedName);

        if (graph.GetLinks(sourceNode, targetNode).Length > 0)
        {
            throw new AssertionException($"{sourceNode.Uid} has unexpected link to {targetNode.Uid}");
        }
    }
}