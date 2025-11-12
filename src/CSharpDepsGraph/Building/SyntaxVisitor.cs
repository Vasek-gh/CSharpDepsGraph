using System;
using System.Collections.Generic;
using System.Linq;
using CSharpDepsGraph.Building.Entities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Building;

internal class SyntaxVisitor : CSharpSyntaxWalker
{
    private readonly ILogger _logger;
    private readonly SemanticModel _semanticModel;
    private readonly ISymbolIdGenerator _symbolIdBuilder;
    private readonly LinkedSymbolsMap _linkedSymbolsMap;
    private readonly Stack<string> _parentIdsStack;
    private readonly bool _fileIsGenerated;

    public SyntaxVisitor(
        ILogger logger,
        SemanticModel semanticModel,
        ISymbolIdGenerator symbolIdBuilder,
        LinkedSymbolsMap linkedSymbolsMap,
        bool fileIsGenerated
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _semanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
        _symbolIdBuilder = symbolIdBuilder ?? throw new ArgumentNullException(nameof(symbolIdBuilder));
        _linkedSymbolsMap = linkedSymbolsMap ?? throw new ArgumentNullException(nameof(linkedSymbolsMap));
        _fileIsGenerated = fileIsGenerated;
        _parentIdsStack = new Stack<string>();
    }

    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        // Type alias can hold symbols for other types(for example generics), because
        // roslyn give us link that ignore alias, we can ignore all 'using directives'
    }

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
        BeginHandleSymbol(node);

        HandleAttributes(node.AttributeLists);
        VisitNodes<CSharpSyntaxNode>(node.BaseList?.Types);

        foreach (var member in node.Members)
        {
            member.Accept(this);
        }

        EndHandleSymbol();
    }

    public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
    {
        BeginHandleSymbol(node);

        HandleConstraints(node.ConstraintClauses);
        HandleAttributes(node.AttributeLists);
        HandleParameterList(node.ParameterList);
        node.ReturnType?.Accept(this);

        EndHandleSymbol();
    }

    public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
    {
        VisitTypeDeclarationSyntax(node, () =>
        {
            if (node.ParameterList?.Parameters.Any() != true)
            {
                return;
            }

            var symbol = _semanticModel.GetDeclaredSymbol(node)
                ?? throw new Exception($"Fail to find record symbol for: {node}");

            var primaryConstructor = symbol.Constructors.SingleOrDefault(x =>
                !x.IsImplicitlyDeclared
                && x.DeclaringSyntaxReferences.SingleOrDefault()?.GetSyntax() is RecordDeclarationSyntax
                )
                ?? throw new Exception($"Primary contructor not found for: {_symbolIdBuilder.Execute(symbol)}");

            BeginHandleSymbol(primaryConstructor);
            HandleParameterList(node.ParameterList);
            EndHandleSymbol();
        });
    }

    private void VisitTypeDeclarationSyntax(TypeDeclarationSyntax node, Action? advancedHandling = null)
    {
        BeginHandleSymbol(node);

        advancedHandling?.Invoke();

        HandleAttributes(node.AttributeLists);
        HandleConstraints(node.ConstraintClauses);
        VisitNodes<CSharpSyntaxNode>(node.BaseList?.Types);

        foreach (var member in node.Members)
        {
            member.Accept(this);
        }

        EndHandleSymbol();
    }

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        foreach (var variable in node.Declaration.Variables)
        {
            BeginHandleSymbol(variable);
            node.Declaration.Type.Accept(this);
            HandleAttributes(node.AttributeLists);
            variable.Initializer?.Accept(this);
            EndHandleSymbol();
        }
    }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        BeginHandleSymbol(node);
        HandleAttributes(node.AttributeLists);
        node.Type.Accept(this);
        node.ExplicitInterfaceSpecifier?.Accept(this);
        node.AccessorList?.Accept(this);
        node.ExpressionBody?.Accept(this);
        node.Initializer?.Accept(this);
        EndHandleSymbol();
    }

    public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
    {
        foreach (var variable in node.Declaration.Variables)
        {
            BeginHandleSymbol(variable);
            node.Declaration.Type.Accept(this);
            HandleAttributes(node.AttributeLists);
            variable.Initializer?.Accept(this);
            EndHandleSymbol();
        }
    }

    public override void VisitEventDeclaration(EventDeclarationSyntax node)
    {
        BeginHandleSymbol(node);

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

        EndHandleSymbol();
    }

    public override void VisitGlobalStatement(GlobalStatementSyntax node)
    {
        BeginHandleSymbol(node.Parent!);
        base.VisitGlobalStatement(node);
        EndHandleSymbol();
    }

    public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        BeginHandleSymbol(node);
        node.Initializer?.Accept(this);
        HandleMethod(node);
        EndHandleSymbol();
    }

    public override void VisitDestructorDeclaration(DestructorDeclarationSyntax node)
    {
        BeginHandleSymbol(node);
        HandleMethod(node);
        EndHandleSymbol();
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        BeginHandleSymbol(node);
        HandleConstraints(node.ConstraintClauses);
        node.ReturnType?.Accept(this);
        node.ExplicitInterfaceSpecifier?.Accept(this);
        HandleMethod(node);
        EndHandleSymbol();
    }

    public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
    {
        HandleConstraints(node.ConstraintClauses);
        node.ReturnType?.Accept(this);
        HandleAttributes(node.AttributeLists);
        HandleParameterList(node.ParameterList);
        node.Body?.Accept(this);
        node.ExpressionBody?.Accept(this);
    }

    public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        // ignore: new T();
        var nodeTypeSymbol = GetSyntaxSymbol(node.Type);
        if (nodeTypeSymbol.Kind == SymbolKind.TypeParameter)
        {
            return;
        }

        ISymbol symbol;
        if (nodeTypeSymbol is ITypeSymbol typeSymbol && typeSymbol.TypeKind == TypeKind.Delegate)
        {
            symbol = nodeTypeSymbol;
        }
        else
        {
            symbol = GetSyntaxSymbol(node);

            // for generics, we need to find the symbol of the type itself, not its instantiation
            if (symbol.ContainingType.IsGenericType)
            {
                symbol = symbol.OriginalDefinition;
            }

            // if constuctor implicitly declared move to parent type
            if (symbol.IsImplicitlyDeclared)
            {
                symbol = symbol.ContainingSymbol;
            }
        }

        HandleSyntaxSymbol(node, symbol);

        VisitNodes<ArgumentSyntax>(node.ArgumentList?.Arguments);
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
            //genericNameSyntax.Accept(this);
            VisitNodes(genericNameSyntax.TypeArgumentList.Arguments);
        }
    }

    public override void VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
    {
        foreach (var initializer in node.Initializers)
        {
            initializer.Expression.Accept(this);
        }
    }

    public override void VisitTupleExpression(TupleExpressionSyntax node)
    {
        foreach (var argument in node.Arguments)
        {
            argument.Expression.Accept(this);
        }
    }

    public override void VisitPredefinedType(PredefinedTypeSyntax node)
    {
        if (node.Keyword.IsKind(SyntaxKind.VoidKeyword))
        {
            return;
        }

        HandleSyntaxSymbol(node);
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

    private void HandleIdentifier(SimpleNameSyntax node)
    {
        if (!HasParentSymbol())
        {
            return;
        }

        if (IsIdentifierShouldBeSkipped(node))
        {
            return;
        }

        var symbol = TryGetSyntaxSymbol(node);
        if (symbol == null)
        {
            return;
        }

        if (IsIdentifierSymbolShouldBeSkipped(node, symbol))
        {
            return;
        }

        if (symbol is INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.IsGenericType)
            {
                symbol = symbol.OriginalDefinition;
            }
        }

        if (symbol is IFieldSymbol fieldSymbol)
        {
            if (fieldSymbol.ContainingType.IsGenericType)
            {
                symbol = symbol.OriginalDefinition;
            }
        }

        if (symbol is IPropertySymbol propertySymbol)
        {
            if (propertySymbol.ContainingType.IsGenericType)
            {
                symbol = symbol.OriginalDefinition;
            }
        }

        if (symbol is IEventSymbol eventSymbol)
        {
            if (eventSymbol.ContainingType.IsGenericType)
            {
                symbol = symbol.OriginalDefinition;
            }
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

        HandleSyntaxSymbol(node, symbol);
    }

    private static bool IsIdentifierShouldBeSkipped(SimpleNameSyntax node)
    {
        // Skip keyword var
        return node.IsVar;
    }

    private static bool IsIdentifierSymbolShouldBeSkipped(SimpleNameSyntax node, ISymbol symbol)
    {
        if (symbol.IsImplicitlyDeclared)
        {
            return true;
        }

        // References to namespaces in expressions are not interesting for us, so we skip
        if (symbol is INamespaceSymbol)
        {
            return true;
        }

        // We skip symbols that are generic types
        if (symbol is ITypeParameterSymbol)
        {
            return true;
        }

        // todo describe !
        if (symbol is IDynamicTypeSymbol)
        {
            return true;
        }

        // ignore linq range variable
        if (symbol is IRangeVariableSymbol)
        {
            return true;
        }

        // ignore local variables symbols
        if (symbol is ILocalSymbol)
        {
            return true;
        }

        // ignore parameters symbols
        if (symbol is IParameterSymbol)
        {
            return true;
        }

        // We ignore references to members of an anonymous class
        if (node.Parent is MemberAccessExpressionSyntax
            && symbol.ContainingSymbol is ITypeSymbol typeSymbol
            && typeSymbol.IsAnonymousType
            )
        {
            return true;
        }

        return false;
    }

    private void BeginHandleSymbol(SyntaxNode syntaxNode)
    {
        var symbol = _semanticModel.GetDeclaredSymbol(syntaxNode)
            ?? throw new Exception($"Symbol for {syntaxNode} not found");

        BeginHandleSymbol(symbol);
    }

    private void BeginHandleSymbol(ISymbol symbol)
    {
        var symbolId = _symbolIdBuilder.Execute(symbol);
        _parentIdsStack.Push(symbolId);
    }

    private void EndHandleSymbol()
    {
        _parentIdsStack.Pop();
    }

    private bool HasParentSymbol()
    {
        return _parentIdsStack.Count > 0;
    }

    private void HandleSyntaxSymbol(SyntaxNode syntax, ISymbol? symbol = null)
    {
        symbol ??= GetSyntaxSymbol(syntax);

        if (_parentIdsStack.Count == 0)
        {
            _logger.LogWarning($"""
                Detect symbol outside parent. Symbol will be skipped.
                Symbol id: {_symbolIdBuilder.Execute(symbol)}.
                Location: {Utils.GetSyntaxLocation(syntax)}.
            """
            );

            return;
        }

        var lineSpan = syntax.SyntaxTree.GetLineSpan(syntax.Span);

        var linkedSymbol = new LinkedSymbol()
        {
            Symbol = symbol,
            Syntax = syntax,
            LocationKind = _fileIsGenerated ? LocationKind.Generated : LocationKind.Local
        };

        _linkedSymbolsMap.Add(_parentIdsStack.Peek(), linkedSymbol);
    }

    private ISymbol GetSyntaxSymbol(SyntaxNode syntax)
    {
        var symbol = _semanticModel.GetSymbolInfo(syntax).Symbol
            ?? throw new Exception($"Symbol not found for {syntax}");

        return symbol;
    }

    private ISymbol? TryGetSyntaxSymbol(SimpleNameSyntax syntax)
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

    private void HandleMethod(BaseMethodDeclarationSyntax node)
    {
        HandleAttributes(node.AttributeLists);
        HandleParameterList(node.ParameterList);
        node.Body?.Accept(this);
        node.ExpressionBody?.Accept(this);
    }

    private void HandleParameterList(ParameterListSyntax? node)
    {
        if (node?.Parameters == null)
        {
            return;
        }

        foreach (var parameter in node.Parameters)
        {
            HandleAttributes(parameter.AttributeLists);
            parameter.Type?.Accept(this);
            parameter.Default?.Accept(this);
        }
    }

    private void HandleConstraints(SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
    {
        VisitNodes(constraintClauses.SelectMany(c => c.Constraints));
    }

    private void HandleAttributes(SyntaxList<AttributeListSyntax> attributes)
    {
        VisitNodes(attributes);
    }

    private void VisitNodes<T>(IEnumerable<T>? nodes) where T : CSharpSyntaxNode
    {
        foreach (var node in nodes ?? [])
        {
            node.Accept(this);
        }
    }
}