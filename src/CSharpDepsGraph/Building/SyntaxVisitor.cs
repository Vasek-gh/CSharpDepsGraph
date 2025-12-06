using CSharpDepsGraph.Building.Entities;
using CSharpDepsGraph.Building.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Building;

internal class SyntaxVisitor : CSharpSyntaxWalker
{
    private readonly ILogger _logger;
    private readonly IFilter _filter;
    private readonly BuildingData _graphData;
    private readonly SemanticModel _semanticModel;
    private readonly bool _isGenerated;
    private readonly string _projectPath;
    private readonly Stack<Node> _nodeStack;
    private readonly Stack<ISymbol> _symbolStack;
    private readonly IAssemblySymbol _assemblySymbol;

    public SyntaxVisitor(
        ILogger logger,
        IFilter filter,
        bool isGenerated,
        string projectPath,
        BuildingData graphData,
        SemanticModel semanticModel
        )
    {
        _logger = logger;
        _filter = filter;
        _isGenerated = isGenerated;
        _projectPath = projectPath;
        _graphData = graphData;
        _semanticModel = semanticModel;

        _nodeStack = new();
        _symbolStack = new();
        _assemblySymbol = semanticModel.Compilation.Assembly;

        _nodeStack.Push(_graphData.Root);
    }

    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        // Type alias can hold symbols for other types(for example generics), because
        // roslyn give us link that ignore alias, we can ignore all 'using directives'
    }

    public override void VisitCompilationUnit(CompilationUnitSyntax syntaxNode)
    {
        var node = PushSymbol(_assemblySymbol);
        _graphData.AddAssemblySyntaxLink(node, _projectPath);

        base.VisitCompilationUnit(syntaxNode);
        PopSymbol();
    }

    public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax syntaxNode)
    {
        var symbol = _semanticModel.GetDeclaredSymbol(syntaxNode)
            ?? throw new Exception($"Namespace symbol not found for {syntaxNode}");

        HandleNamespace(syntaxNode, symbol, () =>
        {
            base.VisitNamespaceDeclaration(syntaxNode);
        });
    }

    public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax syntaxNode)
    {
        var symbol = _semanticModel.GetDeclaredSymbol(syntaxNode)
            ?? throw new Exception($"Namespace symbol not found for {syntaxNode}");

        HandleNamespace(syntaxNode, symbol, () =>
        {
            base.VisitFileScopedNamespaceDeclaration(syntaxNode);
        });
    }

    private void HandleNamespace(BaseNamespaceDeclarationSyntax syntax, INamespaceSymbol symbol, Action action)
    {
        _symbolStack.Clear();

        if (symbol.ConstituentNamespaces.Length > 0)
        {
            symbol = symbol.ConstituentNamespaces.SingleOrDefault(n => n.ContainingAssembly.Name == _assemblySymbol.Name)
                ?? throw new Exception($"ConstituentNamespaces does not have namespace {symbol}");
        }

        Utils.CheckNull(symbol.ContainingAssembly, $"Namespace {symbol} does not have assembly");
        Utils.CheckNull(symbol.ContainingModule, $"Namespace {symbol} does not have module");

        var parentSymbol = symbol.ContainingSymbol;
        while (parentSymbol is not null && parentSymbol.Kind != SymbolKind.Assembly)
        {
            if (
                (parentSymbol.Kind == SymbolKind.Namespace && !parentSymbol.IsGlobalNamespace())
                || (parentSymbol.Kind == SymbolKind.NetModule && parentSymbol.ContainingAssembly.Modules.Count() > 1)
            )
            {
                _symbolStack.Push(parentSymbol);
            }

            parentSymbol = parentSymbol.ContainingSymbol;
        }

        var stackCount = _symbolStack.Count;
        while (_symbolStack.Count > 0)
        {
            PushSymbol(_symbolStack.Pop());
        }

        HandleDeclaration(syntax, symbol, action);

        while (stackCount-- > 0)
        {
            PopSymbol();
        }
    }

    private void HandleDeclaration(SyntaxNode syntaxNode, Action? action = null)
    {
        var symbol = GetDeclaredSymbol(syntaxNode);

        HandleDeclaration(syntaxNode, symbol, action);
    }

    private void HandleDeclaration(SyntaxNode syntaxNode, ISymbol symbol, Action? action = null)
    {
        if (symbol.IsImplicitlyDeclared && !symbol.IsGlobalNamespace())
        {
            return;
        }

        var node = PushSymbol(symbol);
        _graphData.AddSyntaxLink(node, _isGenerated, syntaxNode);

        action?.Invoke();
        PopSymbol();
    }

    private Node PushSymbol(ISymbol symbol)
    {
        var parentNode = _nodeStack.Peek();
        var currentNode = _graphData.AddChildNode(parentNode, symbol);
        _nodeStack.Push(currentNode);

        return currentNode;
    }

    private void PopSymbol()
    {
        var parentNode = _nodeStack.Pop();
    }

    public override void VisitGlobalStatement(GlobalStatementSyntax syntaxNode)
    {
        Utils.CheckNull(syntaxNode.Parent, "Global statement without parent");

        var mainSymbol = GetDeclaredSymbol(syntaxNode.Parent);
        var programSymbol = mainSymbol.ContainingSymbol;

        HandleDeclaration(syntaxNode.Parent, programSymbol, () =>
        {
            HandleDeclaration(syntaxNode.Parent, mainSymbol, () =>
            {
                base.VisitGlobalStatement(syntaxNode);
            });
        });
    }

    //#########################################################################
    //
    // Types
    //
    //#########################################################################

    public override void VisitStructDeclaration(StructDeclarationSyntax node)
    {
        VisitTypeDeclarationSyntax(node);
    }

    public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        VisitTypeDeclarationSyntax(node);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        VisitTypeDeclarationSyntax(node);
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        HandleDeclaration(node, () =>
        {
            HandleNodes(node.AttributeLists);
            HandleNodes<CSharpSyntaxNode>(node.BaseList?.Types);

            foreach (var member in node.Members)
            {
                HandleDeclaration(member, () =>
                {
                    member.Accept(this);
                });
            }
        });
    }

    public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
    {
        HandleDeclaration(node, () =>
        {
            HandleConstraints(node.ConstraintClauses);
            HandleNodes(node.AttributeLists);
            HandleParameterList(node.ParameterList);
            node.ReturnType?.Accept(this);
        });
    }

    public override void VisitRecordDeclaration(RecordDeclarationSyntax syntaxNode)
    {
        VisitTypeDeclarationSyntax(syntaxNode, () =>
        {
            if (syntaxNode.ParameterList is null || syntaxNode.ParameterList.Parameters.Count == 0)
            {
                return;
            }

            var record = _semanticModel.GetDeclaredSymbol(syntaxNode)
                ?? throw new Exception($"Fail to find record symbol for: {syntaxNode}");

            var primaryConstructor = record.Constructors.SingleOrDefault(x =>
                !x.IsImplicitlyDeclared
                && x.DeclaringSyntaxReferences.SingleOrDefault()?.GetSyntax() is RecordDeclarationSyntax
                )
                ?? throw new Exception($"Primary contructor not found for: {record}");

            HandleDeclaration(syntaxNode, primaryConstructor, () =>
            {
                HandleParameterList(syntaxNode.ParameterList);
            });

            var parameters = primaryConstructor.Parameters.Select(p => p.Name).ToHashSet();
            var properties = record.GetMembers().Where(m => m.Kind == SymbolKind.Property && parameters.Contains(m.Name));
            foreach (var property in properties)
            {
                var parameterSyntax = syntaxNode.ParameterList.Parameters
                    .Single(p => p.Identifier.ValueText == property.Name);

                var node = PushSymbol(property);
                _graphData.AddSyntaxLink(node, _isGenerated, parameterSyntax);
                PopSymbol();
            }
        });
    }

    private void VisitTypeDeclarationSyntax(TypeDeclarationSyntax node, Action? advancedHandling = null)
    {
        HandleDeclaration(node, () =>
        {
            advancedHandling?.Invoke();

            HandleNodes(node.AttributeLists);
            HandleConstraints(node.ConstraintClauses);
            HandleNodes<CSharpSyntaxNode>(node.BaseList?.Types);

            foreach (var member in node.Members)
            {
                member.Accept(this);
            }
        });
    }

    //#########################################################################
    //
    // Members
    //
    //#########################################################################

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        foreach (var variable in node.Declaration.Variables)
        {
            HandleDeclaration(variable, () =>
            {
                node.Declaration.Type.Accept(this);
                HandleNodes(node.AttributeLists);
                variable.Initializer?.Accept(this);
            });
        }
    }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        HandleDeclaration(node, () =>
        {
            HandleNodes(node.AttributeLists);
            node.Type.Accept(this);
            node.ExplicitInterfaceSpecifier?.Accept(this);
            node.AccessorList?.Accept(this);
            node.ExpressionBody?.Accept(this);
            node.Initializer?.Accept(this);
        });
    }

    public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
    {
        HandleDeclaration(node, () =>
        {
            HandleNodes(node.AttributeLists);
            HandleParameterList(node.ParameterList);
            node.Type.Accept(this);
            node.ExplicitInterfaceSpecifier?.Accept(this);
            node.AccessorList?.Accept(this);
            node.ExpressionBody?.Accept(this);
        });
    }

    public override void VisitEventDeclaration(EventDeclarationSyntax node)
    {
        HandleDeclaration(node, () =>
        {
            node.Type?.Accept(this);
            node.ExplicitInterfaceSpecifier?.Accept(this);

            if (node.AccessorList != null)
            {
                foreach (var accessor in node.AccessorList.Accessors)
                {
                    accessor.Body?.Accept(this);
                    accessor.ExpressionBody?.Accept(this);
                }
            }
        });
    }

    public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
    {
        foreach (var variable in node.Declaration.Variables)
        {
            HandleDeclaration(variable, () =>
            {
                node.Declaration.Type.Accept(this);
                HandleNodes(node.AttributeLists);
                variable.Initializer?.Accept(this);
            });
        }
    }

    public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        HandleDeclaration(node, () =>
        {
            node.Initializer?.Accept(this);
            HandleMethod(node);
        });
    }

    public override void VisitDestructorDeclaration(DestructorDeclarationSyntax node)
    {
        HandleDeclaration(node, () =>
        {
            HandleMethod(node);
        });
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        HandleDeclaration(node, () =>
        {
            HandleConstraints(node.ConstraintClauses);
            node.ReturnType?.Accept(this);
            node.ExplicitInterfaceSpecifier?.Accept(this);
            HandleMethod(node);
        });
    }

    public override void VisitOperatorDeclaration(OperatorDeclarationSyntax node)
    {
        HandleDeclaration(node, () =>
        {
            node.ReturnType?.Accept(this);
            node.ExplicitInterfaceSpecifier?.Accept(this);
            HandleMethod(node);
        });
    }

    public override void VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
    {
        HandleDeclaration(node, () =>
        {
            node.Type?.Accept(this);
            node.ExplicitInterfaceSpecifier?.Accept(this);
            HandleMethod(node);
        });
    }

    public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
    {
        HandleConstraints(node.ConstraintClauses);
        node.ReturnType?.Accept(this);
        HandleNodes(node.AttributeLists);
        HandleParameterList(node.ParameterList);
        node.Body?.Accept(this);
        node.ExpressionBody?.Accept(this);
    }

    private void HandleMethod(BaseMethodDeclarationSyntax node)
    {
        HandleNodes(node.AttributeLists);
        HandleParameterList(node.ParameterList);
        node.Body?.Accept(this);
        node.ExpressionBody?.Accept(this);
    }

    private void HandleParameterList(BaseParameterListSyntax? node)
    {
        if (node?.Parameters == null)
        {
            return;
        }

        foreach (var parameter in node.Parameters)
        {
            HandleParameter(parameter);
        }
    }

    private void HandleParameter(ParameterSyntax node)
    {
        HandleNodes(node.AttributeLists);
        node.Type?.Accept(this);
        node.Default?.Accept(this);
    }

    //#########################################################################
    //
    // Expressions
    //
    //#########################################################################

    public override void VisitTupleExpression(TupleExpressionSyntax node)
    {
        foreach (var argument in node.Arguments)
        {
            argument.Expression.Accept(this);
        }
    }

    public override void VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
    {
        foreach (var initializer in node.Initializers)
        {
            initializer.Expression.Accept(this);
        }
    }

    public override void VisitPredefinedType(PredefinedTypeSyntax node)
    {
        if (node.Keyword.IsKind(SyntaxKind.VoidKeyword))
        {
            return;
        }

        LinkSyntaxSymbol(node);
    }

    public override void VisitAttribute(AttributeSyntax node)
    {
        var syntax = node.Name;
        var symbol = GetSyntaxSymbol(syntax);

        HandleConstructorSyntax(syntax, symbol);
        HandleNodes<AttributeArgumentSyntax>(node.ArgumentList?.Arguments);
    }

    public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        // todo похоже весь код тут нужен только для того что бы обработать ситуацию с делгатом
        // или IsImplicitlyDeclared конструкторами. Наверно можно тут просто обработать два этих специфических
        // случая отдельно, а для остального сделать как и для всех

        var syntaxSymbol = GetSyntaxSymbol(node.Type);

        // ignore: new T();
        if (syntaxSymbol.Kind == SymbolKind.TypeParameter)
        {
            return;
        }

        // unexpected type symbol
        if (syntaxSymbol is not INamedTypeSymbol ctorTypeSymbol)
        {
            // todo warning
            return;
        }

        // we need to get a constructor symbol. But delegates don't have a constructor,
        // so we only need a type for them.
        var ctorSymbol = ctorTypeSymbol.TypeKind == TypeKind.Delegate
            ? ctorTypeSymbol
            : GetSyntaxSymbol(node);

        // for generics, we need to find the symbol of the type itself, not its instantiation
        if (ctorTypeSymbol.IsGenericType)
        {
            ctorSymbol = ctorSymbol.OriginalDefinition;
        }

        // if the constructor is declared implicitly, we make a reference to its type
        if (ctorSymbol.IsImplicitlyDeclared)
        {
            ctorSymbol = ctorSymbol.ContainingSymbol;
        }

        LinkSyntaxSymbol(node, ctorSymbol);

        HandleNodes<ArgumentSyntax>(node.ArgumentList?.Arguments);
        node.Initializer?.Accept(this);

        var nodeType = node.Type;

        if (nodeType is QualifiedNameSyntax qualifiedNameSyntax)
        {
            qualifiedNameSyntax.Left.Accept(this);
            if (qualifiedNameSyntax.Right is GenericNameSyntax)
            {
                nodeType = qualifiedNameSyntax.Right;
            }
        }

        if (nodeType is GenericNameSyntax genericNameSyntax)
        {
            HandleNodes(genericNameSyntax.TypeArgumentList.Arguments);
        }
    }

    private void HandleConstructorSyntax(TypeSyntax typeSyntax, ISymbol symbol)
    {
        if (symbol.IsImplicitlyDeclared)
        {
            symbol = symbol.ContainingType;
        }

        HandleIdentifier(typeSyntax, symbol);

        if (typeSyntax is QualifiedNameSyntax qualifiedNameSyntax)
        {
            qualifiedNameSyntax.Left.Accept(this);
            if (qualifiedNameSyntax.Right is GenericNameSyntax)
            {
                typeSyntax = qualifiedNameSyntax.Right;
            }
        }

        if (typeSyntax is GenericNameSyntax genericNameSyntax)
        {
            HandleNodes(genericNameSyntax.TypeArgumentList.Arguments);
        }
    }

    public override void VisitGenericName(GenericNameSyntax node)
    {
        HandleIdentifier(node);
        foreach (var typeArgument in node.TypeArgumentList.Arguments)
        {
            typeArgument.Accept(this);
        }
    }

    public override void VisitIdentifierName(IdentifierNameSyntax node)
    {
        HandleIdentifier(node);
    }

    //#########################################################################
    //
    // Utils
    //
    //#########################################################################

    private void HandleConstraints(SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
    {
        HandleNodes(constraintClauses.SelectMany(c => c.Constraints));
    }

    private void HandleNodes<T>(IEnumerable<T>? nodes) where T : CSharpSyntaxNode
    {
        foreach (var node in nodes ?? [])
        {
            node.Accept(this);
        }
    }

    private void HandleIdentifier(TypeSyntax node)
    {
        if (_nodeStack.Count == 0)
        {
            return;
        }

        var symbol = TryGetSyntaxSymbol(node);
        if (symbol == null)
        {
            return;
        }

        HandleIdentifier(node, symbol);
    }

    private void HandleIdentifier(TypeSyntax node, ISymbol symbol)
    {
        if (IsIdentifierSymbolShouldBeSkipped(node, symbol))
        {
            return;
        }

        if (symbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType)
        {
            symbol = symbol.OriginalDefinition;
        }

        if (symbol is IFieldSymbol fieldSymbol && fieldSymbol.ContainingType.IsGenericType)
        {
            symbol = symbol.OriginalDefinition;
        }

        if (symbol is IPropertySymbol propertySymbol && propertySymbol.ContainingType.IsGenericType)
        {
            symbol = symbol.OriginalDefinition;
        }

        if (symbol is IEventSymbol eventSymbol && eventSymbol.ContainingType.IsGenericType)
        {
            symbol = symbol.OriginalDefinition;
        }

        if (symbol is IMethodSymbol methodSymbol)
        {
            // Ignore local function call
            if (methodSymbol.MethodKind == MethodKind.LocalFunction)
            {
                return;
            }

            // При вызове метода расширения roslyn делает обертку на оригинальным методом
            // расширения, поэтому надо достать оригинал перед продолжением
            if (methodSymbol.MethodKind == MethodKind.ReducedExtension)
            {
                symbol = methodSymbol.ReducedFrom ?? methodSymbol;
            }
            else if (methodSymbol.IsGenericMethod)
            {
                symbol = methodSymbol.OriginalDefinition;
            }
            else if (methodSymbol.ContainingType.IsGenericType)
            {
                symbol = methodSymbol.OriginalDefinition;
            }
        }

        LinkSyntaxSymbol(node, symbol);
    }

    private static bool IsIdentifierSymbolShouldBeSkipped(TypeSyntax node, ISymbol symbol)
    {
        // Skip keyword var
        if (node.IsVar)
        {
            return true;
        }

        // At this point, all implicitly declared symbols must be redirected to real symbols
        if (symbol.IsImplicitlyDeclared)
        {
            return true;
        }

        // Namespaces are never referenced
        if (symbol is INamespaceSymbol)
        {
            return true;
        }

        // Skip symbols that are generic types
        if (symbol is ITypeParameterSymbol)
        {
            return true;
        }

        // Ignore dynamic special symbol
        if (symbol is IDynamicTypeSymbol)
        {
            return true;
        }

        // Ignore linq range variable
        if (symbol is IRangeVariableSymbol)
        {
            return true;
        }

        // Ignore local variables symbols
        if (symbol is ILocalSymbol)
        {
            return true;
        }

        // Ignore parameters symbols
        if (symbol is IParameterSymbol)
        {
            return true;
        }

        // Ignore references to members of an anonymous class
        if (node.Parent is MemberAccessExpressionSyntax
            && symbol.ContainingSymbol is ITypeSymbol typeSymbol
            && typeSymbol.IsAnonymousType
            )
        {
            return true;
        }

        return false;
    }

    private void LinkSyntaxSymbol(SyntaxNode syntax, ISymbol? symbol = null)
    {
        symbol ??= GetSyntaxSymbol(syntax);

        if (_nodeStack.Count == 0)
        {
            _logger.LogWarning($"""
                Detect symbol outside parent. Symbol will be skipped.
                Symbol id: {symbol}.
                Location: {Utils.GetSyntaxLocation(syntax)}.
            """
            );

            return;
        }

        var node = _nodeStack.Peek();
        if (node.Symbol is not null && _filter.FilterLinkTarget(node.Symbol, symbol))
        {
            return;
        }

        _graphData.AddLinkedSymbol(
            _nodeStack.Peek(),
            symbol,
            syntax,
            _isGenerated ? LocationKind.Generated : LocationKind.Regular
            );
    }

    private ISymbol GetSyntaxSymbol(SyntaxNode syntax)
    {
        var symbol = _semanticModel.GetSymbolInfo(syntax).Symbol
            ?? throw new Exception($"Syntax symbol not found for {syntax}");

        return symbol;
    }

    private ISymbol GetDeclaredSymbol(SyntaxNode syntax)
    {
        var symbol = _semanticModel.GetDeclaredSymbol(syntax)
            ?? throw new Exception($"Declared symbol not found for {syntax}");

        return symbol;
    }

    private ISymbol? TryGetSyntaxSymbol(TypeSyntax syntax)
    {
        var info = _semanticModel.GetSymbolInfo(syntax);
        if (info.Symbol != null)
        {
            return info.Symbol;
        }

        if (info.CandidateSymbols.Length == 1)
        {
            return info.CandidateSymbols[0];
        }

        return null;
    }
}