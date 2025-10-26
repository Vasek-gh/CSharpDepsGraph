using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

using LinkedSymbolsMap = System.Collections.Generic.IReadOnlyDictionary<
    string,
    System.Collections.Generic.ICollection<CSharpDepsGraph.Building.LinkedSymbol>
    >;

namespace CSharpDepsGraph.Building;

internal class SymbolVisitor : Microsoft.CodeAnalysis.SymbolVisitor
{
    private readonly ILogger _logger;
    private readonly ISet<string> _generatedFiles;
    private readonly ISymbolIdBuilder _symbolIdBuilder;
    private readonly LinkedSymbolsMap _linkedSymbolsMap;
    private readonly GraphData _graphData;
    private readonly Stack<Node> _nodeStack;

    public SymbolVisitor(
        ILogger logger,
        ISet<string> generatedFiles,
        ISymbolIdBuilder symbolIdBuilder,
        LinkedSymbolsMap linkedSymbolsMap,
        GraphData graphData
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _generatedFiles = generatedFiles ?? throw new ArgumentNullException(nameof(generatedFiles));
        _symbolIdBuilder = symbolIdBuilder ?? throw new ArgumentNullException(nameof(symbolIdBuilder));
        _linkedSymbolsMap = linkedSymbolsMap ?? throw new ArgumentNullException(nameof(linkedSymbolsMap));
        _graphData = graphData ?? throw new ArgumentNullException(nameof(graphData));

        _nodeStack = new Stack<Node>();

        _nodeStack.Push(_graphData.Root);
    }

    public override void VisitAssembly(IAssemblySymbol symbol)
    {
        Handle(symbol, true, () =>
        {
            foreach (var module in symbol.Modules)
            {
                Handle(module, symbol.Modules.Count() > 1, () =>
                {
                    Visit(module.GlobalNamespace);
                });
            }
        });
    }

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        Handle(symbol, !symbol.IsGlobalNamespace, () =>
        {
            Visit(symbol.GetMembers());
        });
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        Handle(symbol, true, () =>
        {
            Visit(symbol.GetMembers());
        });
    }

    public override void VisitMethod(IMethodSymbol symbol)
    {
        if (symbol.MethodKind == MethodKind.PropertyGet
            || symbol.MethodKind == MethodKind.PropertySet
            || symbol.MethodKind == MethodKind.EventAdd
            || symbol.MethodKind == MethodKind.EventRemove
            )
        {
            return;
        }

        Handle(symbol, true);
    }

    public override void VisitEvent(IEventSymbol symbol)
    {
        Handle(symbol, true);
    }

    public override void VisitField(IFieldSymbol symbol)
    {
        Handle(symbol, true);
    }

    public override void VisitProperty(IPropertySymbol symbol)
    {
        Handle(symbol, true);
    }

    //##########################################################
    //
    // Utils
    //
    //##########################################################

    private void Visit(IEnumerable<ISymbol> items)
    {
        foreach (var item in items)
        {
            item.Accept(this);
        }
    }

    private void Handle(ISymbol symbol, bool visible, Action? action = null)
    {
        if (symbol.IsImplicitlyDeclared && !symbol.IsGlobalNamespace())
        {
            return;
        }

        if (visible)
        {
            PushSymbol(symbol);
        }

        action?.Invoke();

        if (visible)
        {
            PopSymbol();
        }
    }

    private void PushSymbol(ISymbol roslynSymbol)
    {
        var node = CreateNode(roslynSymbol);
        if (_nodeStack.Count > 0)
        {
            _graphData.AddNode(_logger, _nodeStack.Peek(), node);
        }

        _nodeStack.Push(node);
    }

    private void PopSymbol()
    {
        _nodeStack.Pop();
    }

    private Node CreateNode(ISymbol symbol)
    {
        var id = _symbolIdBuilder.Execute(symbol);
        _linkedSymbolsMap.TryGetValue(id, out var linkedSymbols);

        return new Node()
        {
            Id = id,
            Symbol = symbol,
            SyntaxLinks = GetSyntaxLinks(symbol),
            LinkedSymbols = linkedSymbols ?? Array.Empty<LinkedSymbol>()
        };
    }

    private IEnumerable<SyntaxLink> GetSyntaxLinks(ISymbol symbol)
    {
        if (symbol is IAssemblySymbol)
        {
            return Utils.CreateAssemblySyntaxLink(symbol, SyntaxFileKind.Local);
        }

        var result = new List<SyntaxLink>();
        var syntaxRefs = symbol.GetSyntaxReference();

        foreach (var syntaxRef in syntaxRefs)
        {
            var lineSpan = syntaxRef.SyntaxTree.GetLineSpan(syntaxRef.Span);

            var syntaxLink = Utils.CreateSyntaxLink(
                syntaxRef.GetSyntax(),
                _generatedFiles.Contains(lineSpan.Path) ? SyntaxFileKind.Generated : SyntaxFileKind.Local,
                lineSpan
            );

            result.Add(syntaxLink);
        }

        return result;
    }
}