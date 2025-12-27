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
        var result = AppendSymbolChain(symbols, symbol);

        return result;
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

    private Node? AppendSymbolChain(Stack<ISymbol> symbols, ISymbol originalSymbol)
    {
        if (symbols.Peek() is not IAssemblySymbol assemblySymbol)
        {
            _logger.LogWarning($"""
                A chain was built for the symbol that does not start with the assembly. This symbol will be skipped.
                Symbol: {originalSymbol}.
                Parent: {originalSymbol.ContainingSymbol}.
                Root: {symbols.Peek()}
                """);

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
                    var generatedFileKind = _generatedCodeDetector.GetFileKind(sr.SyntaxTree, CancellationToken.None);
                    if (generatedFileKind == GeneratedFileKind.None)
                    {
                        var location = sr.SyntaxTree.GetLocation(sr.Span);
                        var lineSpan = location.GetLineSpan();
                        var line = lineSpan.StartLinePosition.Line + 1;
                        var column = lineSpan.StartLinePosition.Character + 1;

                        _logger.LogWarning($"""
                            For a symbol that is not in the metadata, the location was defined as regular
                            Symbol: {symbol}.
                            Location: {sr.SyntaxTree.FilePath}:{line}:{column}.
                            """);
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