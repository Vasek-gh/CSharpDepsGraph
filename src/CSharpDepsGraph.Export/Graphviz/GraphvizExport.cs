using DotNetGraph.Compilation;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpDepsGraph.Export.Graphviz;

/// <summary>
/// todo
/// </summary>
public class GraphvizExport
{
    private readonly ILogger<GraphvizExport> _logger;
    private readonly Dictionary<string, DotElement> _nodeMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphvizExport"/> class.
    /// </summary>
    public GraphvizExport(ILogger<GraphvizExport> logger)
    {
        _logger = logger;
        _nodeMap = new Dictionary<string, DotElement>();
    }

    /// <summary>
    /// todo
    /// </summary>
    public Task Run(IGraph graph, Stream stream, CancellationToken cancellationToken)
    {
        // todo currently DotNetGraph does not support CancellationToken
        cancellationToken.ThrowIfCancellationRequested();

        var dotGraph = new DotGraph().WithIdentifier(nameof(CSharpDepsGraph));
        dotGraph.Directed = true;

        AddNodes(dotGraph, graph.Root.Childs.Where(n => !n.IsExternalsRoot()));
        AddNodes(dotGraph, graph.Root.Childs.SingleOrDefault(n => n.IsExternalsRoot())?.Childs
            ?? Array.Empty<INode>()
            );

        var edges = new HashSet<string>();
        foreach (var link in graph.Links)
        {
            AddEdge(dotGraph, link, edges);
        }

        return WriteFile(stream, dotGraph);
    }

    private void AddNodes(DotGraph dotGraph, IEnumerable<INode> nodes)
    {
        foreach (var node in nodes)
        {
            var dotNode = AddNode(node);
            if (dotNode != null)
            {
                dotGraph.Add(dotNode);
            }
        }
    }

    private DotElement AddNode(INode node)
    {
        _logger.LogTrace($"Create node: {node.Id}...");

        var nodeColor = GetNodeColor(node);

        var dotNode = new DotNode()
            .WithIdentifier(node.Id)
            .WithLabel(node.GetCaption())
            .WithShape(DotNodeShape.Box)
            .WithStyle(DotNodeStyle.Filled)
            .WithColor(nodeColor)
            .WithFillColor(nodeColor)
            .WithFontColor(Color.White.AlphaLastStr)
            ;

        _nodeMap.Add(node.Id, dotNode);

        return dotNode;
    }

    private void AddEdge(DotBaseGraph dotGraph, ILink link, HashSet<string> edges)
    {
        _logger.LogTrace($"Write link: {link.Source.Id} -> {link.Target.Id}");

        var srcNodeDotElement = GetDotElement(link.Source);
        var dstNodeDotElement = GetDotElement(link.Target);
        if (srcNodeDotElement == null || dstNodeDotElement == null)
        {
            return;
        }

        var srcDotId = GetDotNodeIdentifier(srcNodeDotElement);
        var dstDotId = GetDotNodeIdentifier(dstNodeDotElement);

        var edgeId = $"{srcDotId.origId}-{dstDotId.origId}";
        if (edges.Contains(edgeId) || srcDotId.origId == dstDotId.origId)
        {
            return;
        }

        var dotEdge = new DotEdge()
            .From(srcDotId.origId)
            .To(dstDotId.origId);

        if (srcDotId.clusterId != null)
        {
            dotEdge.WithAttribute("ltail", $"\"{srcDotId.clusterId}\"");
        }

        if (dstDotId.clusterId != null)
        {
            dotEdge.WithAttribute("lhead", $"\"{dstDotId.clusterId}\"");
        }

        edges.Add(edgeId);
        dotGraph.Add(dotEdge);
    }

    private static (string origId, string? clusterId) GetDotNodeIdentifier(DotElement dotElement)
    {
        if (dotElement is DotNode dotNode)
        {
            return (dotNode.Identifier.Value, null);
        }

        if (dotElement is DotSubgraph dotSubgraph
            && dotSubgraph.Elements.FirstOrDefault() is DotNode dotSubgraphDummyNode
            )
        {
            return (dotSubgraphDummyNode.Identifier.Value, dotSubgraph.Identifier.Value);
        }

        throw new ArgumentException("Unknown class", nameof(dotElement));
    }

    private DotElement? GetDotElement(INode node)
    {
        _nodeMap.TryGetValue(node.Id, out var dotElement);

        return dotElement;
    }

    private static string GetNodeColor(INode node)
    {
        return node.GetNodeType().GetColor().AlphaLastStr;
    }

    private static async Task WriteFile(Stream stream, DotGraph dotGraph)
    {
        using var writer = new StreamWriter(stream);

        var context = new CompilationContext(writer, new DotNetGraph.Compilation.CompilationOptions());

        await dotGraph.CompileAsync(context);
    }
}