using NUnit.Framework;

namespace CSharpDepsGraph.Tests;

internal static class GraphAssert
{
    public static void HasSymbol(IGraph graph, string fullQualifiedName)
    {
        graph.GetNode(AsmName.Test, fullQualifiedName);
    }

    public static void HasNotSymbol(IGraph graph, params (string assemblyName, string fullQualifiedName)[] paths)
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
                throw new AssertionException($"Unexpected symbol {child.Id}");
            }
        }
    }

    public static void HasLink(
        IGraph graph,
        string testAsmFullQualifiedName,
        params (string assemblyName, string fullQualifiedName)[] targets
        )
    {
        HasLink(graph, (AsmName.Test, testAsmFullQualifiedName), targets);
    }

    public static void HasLink(
        IGraph graph,
        (string assemblyName, string fullQualifiedName) source,
        params (string assemblyName, string fullQualifiedName)[] targets
        )
    {
        var sourceNode = graph.GetNode(source.assemblyName, source.fullQualifiedName);
        foreach (var target in targets)
        {
            var targetNode = graph.GetNode(target.assemblyName, target.fullQualifiedName);
            HasLink(graph, sourceNode, targetNode);
        }
    }

    public static void HasLink(IGraph graph, INode source, INode target)
    {
        if (!graph.HasLink(source, target))
        {
            throw new AssertionException($"{source.Id} has link to {target.Id}");
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

        if (graph.HasLink(sourceNode, targetNode))
        {
            throw new AssertionException($"{sourceNode.Id} has link to {targetNode.Id}");
        }
    }
}