using System.Collections.Immutable;
using CSharpDepsGraph.Building.Entities;
using CSharpDepsGraph.Building.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Building;

internal class SymbolVisitor : Microsoft.CodeAnalysis.SymbolVisitor
{
    private readonly ILogger _logger;
    private readonly string _projectPath;
    private readonly ISet<string> _generatedFiles;
    private readonly ISymbolIdGenerator _symbolIdBuilder;
    private readonly LinkedSymbolsMap _linkedSymbolsMap;
    private readonly GraphData _graphData;
    private readonly Stack<string> _nodeStack;

    public SymbolVisitor(
        ILogger logger,
        string projectPath,
        ISet<string> generatedFiles,
        ISymbolIdGenerator symbolIdBuilder,
        LinkedSymbolsMap linkedSymbolsMap,
        GraphData graphData
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _projectPath = projectPath;
        _generatedFiles = generatedFiles ?? throw new ArgumentNullException(nameof(generatedFiles));
        _symbolIdBuilder = symbolIdBuilder ?? throw new ArgumentNullException(nameof(symbolIdBuilder));
        _linkedSymbolsMap = linkedSymbolsMap ?? throw new ArgumentNullException(nameof(linkedSymbolsMap));
        _graphData = graphData ?? throw new ArgumentNullException(nameof(graphData));

        _nodeStack = new Stack<string>();

        _nodeStack.Push(_graphData.Root.Id);
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
        if (symbol.DeclaringSyntaxReferences.All(s => GeneratedCodeUtilities.IsGeneratedCode(s.SyntaxTree, CancellationToken.None)))
        {
            return; // todo
        }

        Handle(symbol, !symbol.IsGlobalNamespace, () =>
        {
            Visit(symbol.GetMembers());
        });
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        var syntaxRef = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.SyntaxTree;
        if (syntaxRef != null && GeneratedCodeUtilities.IsGeneratedCode(syntaxRef, CancellationToken.None))
        {
            return; // todo
        }

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

    private void PushSymbol(ISymbol symbol)
    {
        if (_nodeStack.Count == 0)
        {
            throw new InvalidOperationException("Node stack is empty");
        }

        var id = _symbolIdBuilder.Execute(symbol);
        var parentId = _nodeStack.Peek();

        var node = _graphData.AddNode(
            _logger,
            parentId,
            id,
            symbol
            );

        if (node is not null)
        {
            var linkedSymbols = _linkedSymbolsMap.Get(id);
            node.AddLinkedSymbols(linkedSymbols);
            AddSyntaxLinks(node, symbol);
        }

        _nodeStack.Push(id);
    }

    private void PopSymbol()
    {
        _nodeStack.Pop();
    }

    private void AddSyntaxLinks(Node node, ISymbol symbol)
    {
        if (symbol is IAssemblySymbol)
        {
            node.AddAssemblySyntaxLink(_projectPath);
            return;
        }

        ForEachSyntaxReference(symbol, (syntaxReference) =>
        {
            var location = syntaxReference.SyntaxTree.FilePath;
            var locationKind = _generatedFiles.Contains(location) ? LocationKind.Generated : LocationKind.Local;

            node.AddSyntaxReference(locationKind, syntaxReference);
        });
    }

    internal static void ForEachSyntaxReference(ISymbol symbol, Action<SyntaxReference> action)
    {
        ForEachSyntaxReference(symbol.DeclaringSyntaxReferences, action);

        if (symbol is IMethodSymbol methodSymbol)
        {
            ForEachSyntaxReference(methodSymbol.PartialDefinitionPart?.DeclaringSyntaxReferences, action);
            ForEachSyntaxReference(methodSymbol.PartialImplementationPart?.DeclaringSyntaxReferences, action);
        }

        if (symbol is IEventSymbol eventSymbol)
        {
            ForEachSyntaxReference(eventSymbol.PartialDefinitionPart?.DeclaringSyntaxReferences, action);
            ForEachSyntaxReference(eventSymbol.PartialImplementationPart?.DeclaringSyntaxReferences, action);
        }

        if (symbol is IPropertySymbol propertySymbol)
        {
            ForEachSyntaxReference(propertySymbol.PartialDefinitionPart?.DeclaringSyntaxReferences, action);
            ForEachSyntaxReference(propertySymbol.PartialImplementationPart?.DeclaringSyntaxReferences, action);
        }
    }

    private static void ForEachSyntaxReference(ImmutableArray<SyntaxReference>? syntaxReferences, Action<SyntaxReference> action)
    {
        if (syntaxReferences is null)
        {
            return;
        }

        foreach (var item in syntaxReferences)
        {
            action(item);
        }
    }
}