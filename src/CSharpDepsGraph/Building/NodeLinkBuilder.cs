using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Building;

internal class NodeLinkBuilder
{
    private readonly ILogger _logger;
    private readonly GraphData _graphData;
    private readonly ISymbolIdBuilder _symbolIdBuilder;

    public NodeLinkBuilder(ILogger<NodeLinkBuilder> logger, ISymbolIdBuilder symbolIdBuilder, GraphData graphData)
    {
        _logger = logger;
        _symbolIdBuilder = symbolIdBuilder;
        _graphData = graphData;
    }

    public void Run()
    {
        _logger.LogInformation("Begin build links...");

        var childNodes = GetRootChilds();
        foreach (var child in childNodes)
        {
            HandleNode(child);
        }
    }

    private void HandleNode(Node node)
    {
        //_logger.LogTrace($"HandleNode: {node.Id}");

        foreach (var linkedSymbol in node.LinkedSymbolsList)
        {
            var symbolId = _symbolIdBuilder.Execute(linkedSymbol.Symbol);
            if (!_graphData.NodeMap.TryGetValue(symbolId, out var targetNode))
            {
                targetNode = CreateExternalNode(linkedSymbol.Symbol, symbolId, node);
            }

            _graphData.Links.Add(new Link()
            {
                Source = node,
                Target = targetNode,
                Syntax = linkedSymbol.Syntax,
                LocationKind = linkedSymbol.LocationKind,
            });
        }
    }

    private Node CreateExternalNode(ISymbol? symbol, string id, Node fromNode)
    {
        symbol = symbol ?? throw new Exception($"Null symbol for {id ?? "unknown id"}");

        //_logger.LogTrace($"Create external node: {id}");

        var parentNode = GetExternalParentNode(symbol, fromNode);

        var node = _graphData.AddNode(_logger, parentNode.Id, id, symbol, Utils.CreateExternalSyntaxLink(symbol))
            ?? throw new Exception($"Create node {id} fail");

        return node;
    }

    private Node GetExternalParentNode(ISymbol symbol, Node fromNode)
    {
        var parentSymbol = symbol.ContainingSymbol;

        if (parentSymbol.IsGlobalNamespace())
        {
            parentSymbol = parentSymbol.ContainingSymbol;
        }

        if (parentSymbol is IModuleSymbol && symbol.ContainingAssembly.Modules.Count() == 1)
        {
            parentSymbol = parentSymbol.ContainingSymbol;
        }

        var parentSymbolId = GetExternalParentSymbolId(symbol, parentSymbol);

        if (!_graphData.NodeMap.TryGetValue(parentSymbolId, out var parentNode))
        {
            return CreateExternalNode(parentSymbol, parentSymbolId, fromNode);
        }

        //_logger.LogTrace($"Parent node already exists: {parentSymbolId}");

        if (!parentNode.IsExternal())
        {
            var symbolSyntax = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
            var symbolLocation = symbolSyntax == null ? "<fail>" : Utils.GetSyntaxLocation(symbolSyntax);

            _logger.LogWarning($"""
                When creating an external node, it was detected that the symbol will be created as a child of the local.
                Source node: {fromNode.Id}.
                Symbol id: {_symbolIdBuilder.Execute(symbol)}.
                Symbol location: {symbolLocation}
                Parent symbol id: {_symbolIdBuilder.Execute(parentSymbol)}.
                """);
        }

        return parentNode;
    }

    private string GetExternalParentSymbolId(ISymbol symbol, ISymbol? parentSymbol)
    {
        var parentSymbolId = _graphData.External.Id;

        if (parentSymbol != null)
        {
            parentSymbolId = _symbolIdBuilder.Execute(parentSymbol);
        }
        else if (symbol is not IAssemblySymbol)
        {
            var symbolId = _symbolIdBuilder.Execute(symbol);
            _logger.LogWarning($"""
                Found a symbol without a parent that is not an assembly.
                The node will be created as a child of the external root.
                Symbol id: {symbolId}.
                """);
        }

        return parentSymbolId;
    }

    private List<Node> GetRootChilds()
    {
        var result = new List<Node>();
        VisitChilds(_graphData.Root, (child) =>
        {
            result.Add(child);
        });

        return result;

        void VisitChilds(Node node, Action<Node> action)
        {
            foreach (var child in node.ChildList)
            {
                action(child);
                VisitChilds(child, action);
            }
        }
    }
}