using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using CSharpDepsGraph.Building;

namespace CSharpDepsGraph.Mutation;

/// <summary>
/// todo
/// </summary>
public class NamespaceOnlyMutator : IMutator
{
    private readonly Dictionary<string, INode> _newNodeIdMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="NamespaceOnlyMutator"/> class.
    /// </summary>
    public NamespaceOnlyMutator()
    {
        _newNodeIdMap = new Dictionary<string, INode>();
    }

    /// <inheritdoc/>
    public IGraph Run(IGraph graph)
    {
        var rootNodes = GetNamespaces(graph.Root);

        var externalNode = graph.Root.Childs.SingleOrDefault(c => c.IsExternalsRoot());
        if (externalNode != null)
        {
            rootNodes = rootNodes.Concat(new[] {
                MutatedNode.Copy(externalNode, GetNamespaces(externalNode))
            });
        }

        return new MutatedGraph()
        {
            Root = MutatedNode.Copy(graph.Root, rootNodes),
            Links = MutateLinks(graph.Links)
        };
    }

    private IEnumerable<INode> GetNamespaces(INode assemblyNodesRoot)
    {
        var namespacesData = CollectNamaspacesData(assemblyNodesRoot);

        var result = new List<INode>();
        foreach (var namespaceData in namespacesData)
        {
            var node = new MutatedNode()
            {
                Id = namespaceData.Id,
                Symbol = namespaceData.Symbols.First(),
                SyntaxLinks = namespaceData.SyntaxLinks,
                Childs = Array.Empty<INode>()
            };

            foreach (var id in namespaceData.OriginalIds)
            {
                _newNodeIdMap.Add(id, node);
            }

            result.Add(node);
        }

        return result;
    }

    private static IEnumerable<NamespaceData> CollectNamaspacesData(INode assemblyNodesRoot)
    {
        var map = new Dictionary<string, NamespaceData>();

        foreach (var assemblyNode in assemblyNodesRoot.Childs.Where(c => c.Symbol is IAssemblySymbol))
        {
            var assemblyData = GetAssemblyData(assemblyNode);

            foreach (var currentNamespaceData in assemblyData)
            {
                if (!map.TryGetValue(currentNamespaceData.Id, out var namespaceData))
                {
                    namespaceData = new NamespaceData(currentNamespaceData.Id);
                    map.Add(namespaceData.Id, namespaceData);
                }

                namespaceData.Symbols.Add(currentNamespaceData.Symbols.Single());
                namespaceData.OriginalIds.AddRange(currentNamespaceData.OriginalIds);
                namespaceData.SyntaxLinks.AddRange(currentNamespaceData.SyntaxLinks);
            }
        }

        return map.Values;
    }

    private static List<NamespaceData> GetAssemblyData(INode assemblyNode)
    {
        if (assemblyNode.Symbol is not IAssemblySymbol assemblySymbol)
        {
            throw new ArgumentException("Node is not assembly", nameof(assemblyNode));
        }

        var result = new List<NamespaceData>();

        var globalNamespaceNodes = assemblyNode.Childs.Where(c => c.Symbol is not INamespaceSymbol);
        if (globalNamespaceNodes.Any())
        {
            var fileKind = assemblyNode.SyntaxLinks.FirstOrDefault()?.FileKind
                ?? throw new Exception($"Missing syntax link fro assembly {assemblySymbol.Name}");

            result.Add(new NamespaceData(
                $"global::{assemblySymbol.Name}",
                assemblySymbol.GlobalNamespace,
                globalNamespaceNodes.SelectMany(c => c.CollectNodeData(c => c.Id)),
                Utils.CreateAssemblySyntaxLink(assemblySymbol, fileKind)
            ));
        }

        foreach (var namespaceNode in assemblyNode.Childs.Where(c => c.Symbol is INamespaceSymbol))
        {
            result.Add(new NamespaceData(
                namespaceNode.Symbol?.ToDisplayString() ?? throw new InvalidOperationException(),
                namespaceNode.Symbol,
                namespaceNode.CollectNodeData(c => c.Id),
                namespaceNode.SyntaxLinks
            ));
        }

        return result;
    }

    private List<ILink> MutateLinks(IEnumerable<ILink> links)
    {
        var result = new List<ILink>();
        foreach (var link in links)
        {
            if (!_newNodeIdMap.TryGetValue(link.Source.Id, out var newSource)
                || !_newNodeIdMap.TryGetValue(link.Target.Id, out var newTarget)
                )
            {
                continue;
            }

            result.Add(MutatedLink.Copy(link, newSource, newTarget));
        }

        return result;
    }

    private class NamespaceData
    {
        public string Id { get; set; }
        public List<ISymbol> Symbols { get; init; } = new List<ISymbol>();
        public List<string> OriginalIds { get; init; } = new List<string>();
        public List<SyntaxLink> SyntaxLinks { get; set; } = new List<SyntaxLink>();

        public NamespaceData(
            string id,
            ISymbol? symbol = null,
            IEnumerable<string>? originalIds = null,
            IEnumerable<SyntaxLink>? syntaxLinks = null
            )
        {
            Id = id;

            if (symbol != null)
            {
                Symbols.Add(symbol);
            }

            if (originalIds != null)
            {
                OriginalIds = originalIds.ToList();
            }

            if (syntaxLinks != null)
            {
                SyntaxLinks = syntaxLinks.ToList();
            }
        }
    }
}