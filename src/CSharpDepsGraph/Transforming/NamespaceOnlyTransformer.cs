using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Transforming;

/// <summary>
/// Leaves only namespace nodes, preserving the relationships between them
/// </summary>
public class NamespaceOnlyTransformer : ITransformer
{
    internal const string GlobalId = "global::";

    private readonly Dictionary<string, INode> _newNodeIdMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="NamespaceOnlyTransformer"/> class.
    /// </summary>
    public NamespaceOnlyTransformer()
    {
        _newNodeIdMap = new Dictionary<string, INode>();
    }

    /// <inheritdoc/>
    public IGraph Execute(IGraph graph)
    {
        var nodes = GetNodes(graph.Root);
        return new MutatedGraph()
        {
            Root = MutatedNode.Copy(graph.Root, nodes),
            Links = MutateLinks(graph.Links)
        };
    }

    private IEnumerable<INode> GetNodes(INode node)
    {
        var namespaceMap = new Dictionary<string, NamespaceNode>();
        NamespaceNode? globalNamespaceData = null;

        node.VisitNodes((node) =>
        {
            if (node.Symbol is IAssemblySymbol assemblySymbol)
            {
                globalNamespaceData ??= new NamespaceNode(GlobalId, assemblySymbol.GlobalNamespace);
                HandleAssemblyNode(node, globalNamespaceData);
                return true;
            }

            if (node.Symbol is INamespaceSymbol)
            {
                HandleNamespaceNode(node, namespaceMap);
                return false;
            }

            return true;
        });


        if (globalNamespaceData?.SyntaxLinkList.Count > 0)
        {
            namespaceMap.Add(globalNamespaceData.Uid, globalNamespaceData);
        }

        return namespaceMap.Values.Select(nd =>
            new MutatedNode()
            {
                Uid = nd.Uid,
                Symbol = nd.Symbol,
                SyntaxLinks = nd.SyntaxLinks,
                Childs = []
            }
        );
    }

    private void HandleAssemblyNode(INode node, NamespaceNode globalNamespaceNode)
    {
        var assemblySymbol = node.Symbol as IAssemblySymbol
            ?? throw new InvalidOperationException();

        var assemblyLocationKind = node.SyntaxLinks.FirstOrDefault()?.LocationKind
            ?? throw new InvalidOperationException($"Missing syntax link for assembly {assemblySymbol.Name}");

        var assemblyGlobalChildNodes = node.Childs.Where(c => c.Symbol is not INamespaceSymbol)
            .SelectMany(c => c.CollectNodeData(c => c));

        var hasNodes = false;
        foreach (var assemblyChildNode in assemblyGlobalChildNodes)
        {
            hasNodes = true;
            _newNodeIdMap.Add(assemblyChildNode.Uid, globalNamespaceNode);
        }

        if (hasNodes)
        {
            globalNamespaceNode.SyntaxLinkList.AddRange(node.SyntaxLinks);
        }
    }

    private void HandleNamespaceNode(INode node, Dictionary<string, NamespaceNode> namespaceMap)
    {
        var namespaceSymbol = node.Symbol as INamespaceSymbol
            ?? throw new InvalidOperationException();

        var namespaceId = namespaceSymbol.ToDisplayString();
        if (!namespaceMap.TryGetValue(namespaceId, out var namespaceNode))
        {
            namespaceNode = new NamespaceNode(namespaceId, namespaceSymbol);
            namespaceMap.Add(namespaceNode.Uid, namespaceNode);
        }

        var namespaceChildNodes = node.Childs.SelectMany(c => c.CollectNodeData(c => c));
        foreach (var namespaceChildNode in namespaceChildNodes)
        {
            _newNodeIdMap.Add(namespaceChildNode.Uid, namespaceNode);
        }

        namespaceNode.SyntaxLinkList.AddRange(node.SyntaxLinks);
    }

    private List<ILink> MutateLinks(IEnumerable<ILink> links)
    {
        var result = new List<ILink>();
        foreach (var link in links)
        {
            if (!_newNodeIdMap.TryGetValue(link.Source.Uid, out var newSource)
                || !_newNodeIdMap.TryGetValue(link.Target.Uid, out var newTarget)
                )
            {
                continue;
            }

            result.Add(MutatedLink.Copy(link, newSource, newTarget));
        }

        return result;
    }

    private class NamespaceNode : INode
    {
        public string Uid { get; }

        public ISymbol? Symbol { get; }

        public IEnumerable<INode> Childs { get; } = [];

        public IEnumerable<INodeSyntaxLink> SyntaxLinks => SyntaxLinkList;

        public List<INodeSyntaxLink> SyntaxLinkList { get; } = new List<INodeSyntaxLink>();

        public NamespaceNode(string id, ISymbol symbol)
        {
            Uid = id;
            Symbol = symbol;
        }
    }
}