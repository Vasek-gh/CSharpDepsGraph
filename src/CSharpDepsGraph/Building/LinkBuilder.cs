using CSharpDepsGraph.Building.Entities;
using CSharpDepsGraph.Building.Services;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace CSharpDepsGraph.Building;

internal class LinkBuilder
{
    private readonly ILogger _logger;
    private readonly BuildingData _graphData;
    private readonly GeneratedCodeDetector _generatedCodeDetector;
    private readonly Dictionary<string, ExternalNodeSyntaxLink> _assemblyLinksCache;

    public LinkBuilder(
        ILogger logger,
        BuildingData graphData,
        GeneratedCodeDetector generatedCodeDetector
        )
    {
        _logger = logger;
        _graphData = graphData;
        _generatedCodeDetector = generatedCodeDetector;
        _assemblyLinksCache = new();
    }

    public void Run()
    {
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

        var isInMetadata = Utils.IsInMetadata(assemblySymbol);

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
                if (symbol.Kind == SymbolKind.Assembly)
                {
                    _graphData.AddExternalSyntaxLink(result, CreateExternalSyntaxLink(symbol));
                }
            }
            else
            {
                ForEachSyntaxReference(symbol, (sr) =>
                {
                    var generatedFileKind = _generatedCodeDetector.GetGeneratedFileKindAsync(sr.SyntaxTree, CancellationToken.None);
                    if (generatedFileKind == GeneratedFileKind.None)
                    {
                        /* todo
                        _logger.LogWarning($"""
                            When creating an external node, it was detected that the symbol will be created as a child of the local.
                            Source node: {fromNode.Id}.
                            Symbol id: {_idGenerator.Execute(symbol)}.
                            Symbol location: {symbolLocation}
                            Parent symbol id: {_idGenerator.Execute(parentSymbol)}.
                            """);
                        */
                    }

                    var syntax = sr.GetSyntax();
                    _graphData.AddSyntaxLink(result, generatedFileKind != GeneratedFileKind.None, syntax);
                });
            }
        }

        return result;
    }

    private ExternalNodeSyntaxLink CreateExternalSyntaxLink(ISymbol symbol)
    {
        var assemblyName = symbol is IAssemblySymbol
            ? symbol.Name
            : symbol.ContainingAssembly.Name;

        if (!_assemblyLinksCache.TryGetValue(assemblyName, out var link))
        {
            link = new ExternalNodeSyntaxLink(assemblyName);
            _assemblyLinksCache.Add(assemblyName, link);
        }

        return link;
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