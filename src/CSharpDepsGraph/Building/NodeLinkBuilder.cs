using CSharpDepsGraph.Building.Entities;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace CSharpDepsGraph.Building;

internal class LinkBuilder
{
    private readonly ILogger _logger;
    private readonly GraphData _graphData;

    public LinkBuilder(
        ILogger logger,
        GraphData graphData
        )
    {
        _logger = logger;
        _graphData = graphData;
    }

    public void Run()
    {
        _logger.LogInformation("Begin build links...");

        VisitNodes(_graphData.Root, n =>
        {
            HandleNode(n);
        });
    }

    private void HandleNode(Node node)
    {
        //_logger.LogTrace($"HandleNode: {node.Id}");

        foreach (var linkedSymbol in node.LinkedSymbolsList)
        {
            var targetNode = CreateNode(linkedSymbol.Symbol);
            if (targetNode is not null)
            {
                _graphData.AddLink(node, targetNode, linkedSymbol.Syntax, linkedSymbol.LocationKind);
            }
        }
    }

    private Node? CreateNode(ISymbol symbol)
    {
        var symbols = new Stack<ISymbol>(10);
        var externalRoot = _graphData.External;

        BuildSymbolChain(symbols, symbol);
        return AppendSymbolChain(symbols);
    }

    private Node? AppendSymbolChain(Stack<ISymbol> symbols)
    {
        if (symbols.Peek() is not IAssemblySymbol assemblySymbol)
        {
            _logger.LogWarning("todo");
            return null;
        }

        var isInMetadata = assemblySymbol.Locations.Length == 1 && assemblySymbol.Locations[0].IsInMetadata;

        var parentNode = isInMetadata
            ? _graphData.External
            : _graphData.Root;

        var result = parentNode;

        while (symbols.Count > 0)
        {
            var symbol = symbols.Pop();
            result = _graphData.AddChildNode(result, symbol, out var newNode);
            if (!newNode)
            {
                continue;
            }

            if (isInMetadata)
            {
                result.SyntaxLinkList = Utils.CreateExternalSyntaxLink(symbol);
            }
            else
            {
                ForEachSyntaxReference(symbol, (sr) =>
                {
                    // todo check generated and warning if not
                    /*
                     _logger.LogWarning($"""
                        When creating an external node, it was detected that the symbol will be created as a child of the local.
                        Source node: {fromNode.Id}.
                        Symbol id: {_idGenerator.Execute(symbol)}.
                        Symbol location: {symbolLocation}
                        Parent symbol id: {_idGenerator.Execute(parentSymbol)}.
                        """);
                    */
                    var syntax = sr.GetSyntax();
                    _graphData.AddSyntaxLink(result, LocationKind.Local, syntax);
                });
            }
        }

        return result;
    }

    internal static void ForEachSyntaxReference(ISymbol symbol, Action<SyntaxReference> action)
    {
        ForEachSyntaxReference(symbol.DeclaringSyntaxReferences, action);

        if (symbol is IMethodSymbol methodSymbol)
        {
            ForEachSyntaxReference(methodSymbol.PartialDefinitionPart?.DeclaringSyntaxReferences, action);
        }

        if (symbol is IEventSymbol eventSymbol)
        {
            ForEachSyntaxReference(eventSymbol.PartialDefinitionPart?.DeclaringSyntaxReferences, action);
        }

        if (symbol is IPropertySymbol propertySymbol)
        {
            ForEachSyntaxReference(propertySymbol.PartialDefinitionPart?.DeclaringSyntaxReferences, action);
        }

        void ForEachSyntaxReference(ImmutableArray<SyntaxReference>? syntaxReferences, Action<SyntaxReference> action)
        {
            foreach (var syntaxReference in syntaxReferences ?? [])
            {
                action(syntaxReference);
            }
        }
    }

    private static void BuildSymbolChain(Stack<ISymbol> symbols, ISymbol symbol)
    {
        symbols.Clear();
        while (symbol is not null)
        {
            symbols.Push(symbol);

            if (symbol.Kind == SymbolKind.Assembly)
            {
                break;
            }

            symbol = symbol.ContainingSymbol;

            if (symbol.IsGlobalNamespace())
            {
                symbol = symbol.ContainingModule;
            }

            if (symbol.Kind == SymbolKind.NetModule && symbol.ContainingAssembly.Modules.Count() == 1)
            {
                symbol = symbol.ContainingAssembly;
            }
        }
    }

    private static void VisitNodes(Node node, Action<Node> action)
    {
        if (node.IsExternalsRoot())
        {
            return;
        }

        var childCount = node.ChildList.Count;
        for (var i = 0; i < childCount; i++)
        {
            var child = node.ChildList[i];
            action(child);
            VisitNodes(child, action);
        }
    }
}