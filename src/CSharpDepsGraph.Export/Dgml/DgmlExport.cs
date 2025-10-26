using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;

namespace CSharpDepsGraph.Export.Dgml;

// todo link category
/// <summary>
/// todo
/// </summary>
public class DgmlExport
{
    private int _idCounter;

    private readonly XNamespace _ns;
    private readonly Dictionary<string, string> _idsMap;
    private readonly ILogger _logger;

    private static readonly Dictionary<NodeType, string> _iconMap = new() {
        { NodeType.Unknown, "" },
        { NodeType.Group, "" },
        { NodeType.Assembly, "CodeSchema_Assembly" },
        { NodeType.Namespace, "CodeSchema_Namespace" },
        { NodeType.Enum, "CodeSchema_Enum" },
        { NodeType.Class, "CodeSchema_Class" },
        { NodeType.Structure, "CodeSchema_Struct" },
        { NodeType.Interface, "CodeSchema_Interface" },
        { NodeType.Const, "CodeSchema_Field" },
        { NodeType.Field, "CodeSchema_Field" },
        { NodeType.Property, "CodeSchema_Property" },
        { NodeType.Method, "CodeSchema_Method" },
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="DgmlExport"/> class.
    /// </summary>
    public DgmlExport(ILogger<DgmlExport> logger)
    {
        _ns = @"http://schemas.microsoft.com/vs/2009/dgml";
        _idsMap = new Dictionary<string, string>();
        _idCounter = 0;
        _logger = logger;
    }

    /// <summary>
    /// todo
    /// </summary>
    public Task Run(IGraph graph, Stream stream, CancellationToken cancellationToken) // todo rename RunAsync + cancellationToken = deafult
    {
        var msaglNodes = new List<XElement>();
        var msgalLinks = new Dictionary<string, XElement>();

        foreach (var node in graph.Root.Childs)
        {
            AddNode(node, msaglNodes, msgalLinks);
        }

        foreach (var link in graph.Links)
        {
            AddEdge(link, msgalLinks);
        }

        var xDoc = new XDocument(
            new XElement(_ns + "DirectedGraph",
                new XElement(_ns + "Nodes", msaglNodes),
                new XElement(_ns + "Links", msgalLinks.Values),
                CreateCategories()
            )
        );

        return xDoc.SaveAsync(stream, SaveOptions.None, cancellationToken);
    }

    private void AddNode(INode node, List<XElement> msaglNodes, Dictionary<string, XElement> msaglLinks)
    {
        _logger.LogTrace($"Add node: {node.Id}...");

        var attrs = new List<XAttribute>() {
            new XAttribute("Id", GetNodeId(node)),
            new XAttribute("Label", node.GetCaption()),
            new XAttribute("Category", GetCategoryName(node.GetNodeType())),
        };

        if (node.Childs.Any())
        {
            attrs.Add(new XAttribute("Group", "Collapsed"));
        }

        msaglNodes.Add(new XElement(_ns + "Node", attrs));

        foreach (var child in node.Childs)
        {
            AddNode(child, msaglNodes, msaglLinks);
            msaglLinks.Add(child.Id, CreateLinkElement(node, child, true));
        }
    }

    private void AddEdge(ILink link, Dictionary<string, XElement> msaglLinks)
    {
        _logger.LogTrace($"Add edge: {link.Source.Id} -> {link.Target.Id}");

        if (link.Source.Id == link.Target.Id)
        {
            return;
        }

        var edgeId = $"{link.Source.Id}-{link.Target.Id}";
        if (msaglLinks.ContainsKey(edgeId))
        {
            return;
        }

        msaglLinks.Add(edgeId, CreateLinkElement(link.Source, link.Target, false));
    }

    private XElement CreateLinkElement(INode source, INode target, bool groupLink)
    {
        var attrs = new List<XAttribute>() {
            new XAttribute("Source", GetNodeId(source)),
            new XAttribute("Target", GetNodeId(target))
        };

        if (groupLink)
        {
            attrs.Add(new XAttribute("Category", "Contains"));
        }

        return new XElement(_ns + "Link", attrs);
    }

    private string GetNodeId(INode node)
    {
        if (!_idsMap.TryGetValue(node.Id, out var result))
        {
            _idCounter++;
            result = _idCounter.ToString(CultureInfo.InvariantCulture);
            _idsMap.Add(node.Id, result);
        }

        return result;
    }

    private XElement CreateCategories()
    {
        return new XElement(_ns + "Categories",
            Enum.GetValues<NodeType>()
                .Select(nt => CreateCategory(
                    GetCategoryName(nt),
                    nt.ToString(),
                    nt.GetColor().AlphaFirstStr,
                    GetIcon(nt)
                ))
                .Append(new XElement(_ns + "Category",
                    new XAttribute("Id", "Contains"),
                    new XAttribute("IsContainment", "True")
                ))
        );
    }

    private XElement CreateCategory(string id, string label, string background, string icon)
    {
        return new XElement(_ns + "Category",
            new XAttribute("Id", id),
            new XAttribute("Label", label),
            new XAttribute("Background", background),
            new XAttribute("Icon", icon)
        );
    }

    private static string GetIcon(NodeType nodeType)
    {
        return _iconMap.TryGetValue(nodeType, out var icon) ? icon : "";
    }

    private static string GetCategoryName(NodeType nodeType)
    {
        return $"{nodeType}Category";
    }
}
