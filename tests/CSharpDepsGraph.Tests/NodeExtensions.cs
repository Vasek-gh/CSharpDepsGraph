using Microsoft.CodeAnalysis;
using NUnit.Framework;
using System.Linq;

namespace CSharpDepsGraph.Tests;

public static class NodeExtensions
{
    public static INode GetNode(this INode node, string path)
    {
        var nodes = GetNodes(node, path);
        if (nodes.Length == 0)
        {
            throw new AssertionException($"{path} not found in {GetNodeName(node)}");
        }
        if (nodes.Length > 1)
        {
            throw new AssertionException($"{GetNodeName(node)} has multiple {path} symbols");
        }

        return nodes[0];
    }

    public static INode[] GetNodes(this INode node, string path)
    {
        var currentNode = node;

        var symbolNames = path.Split("/", System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var name in symbolNames.Take(symbolNames.Length - 1))
        {
            var child = currentNode.Childs.SingleOrDefault(n => CompareNode(n, name));
            if (child is null)
            {
                return [];
            }

            currentNode = child;
        }

        return currentNode.Childs.Where(n => CompareNode(n, symbolNames.Last())).ToArray();
    }

    private static bool CompareNode(INode node, string symbol)
    {
        var nodeName = GetNodeName(node);

        return nodeName == symbol;
    }

    private static string GetNodeName(INode node)
    {
        return node switch
        {
            _ when node.Symbol is IEventSymbol => node.Symbol.ToDisplayString(MemberDisplayFormat),
            _ when node.Symbol is IPropertySymbol => node.Symbol.ToDisplayString(MemberDisplayFormat),
            _ when node.Symbol is IMethodSymbol => node.Symbol.ToDisplayString(MemberDisplayFormat),
            _ when node.Symbol is ITypeSymbol => node.Symbol.ToDisplayString(GeneralDisplayFormat),
            _ when node.Symbol is INamedTypeSymbol => node.Symbol.ToDisplayString(GeneralDisplayFormat),
            _ when node.Symbol is not null => node.Symbol.Name,
            _ => node.Id
        };
    }

    private static SymbolDisplayFormat GeneralDisplayFormat { get; } =
        new SymbolDisplayFormat(
            globalNamespaceStyle:
                SymbolDisplayGlobalNamespaceStyle.Omitted,
            genericsOptions:
                SymbolDisplayGenericsOptions.IncludeTypeParameters,
            kindOptions:
                SymbolDisplayKindOptions.None,
            miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            );

    private static SymbolDisplayFormat MemberDisplayFormat { get; } =
        new SymbolDisplayFormat(
            globalNamespaceStyle:
                SymbolDisplayGlobalNamespaceStyle.Omitted,
            genericsOptions:
                SymbolDisplayGenericsOptions.IncludeTypeParameters |
                SymbolDisplayGenericsOptions.IncludeTypeConstraints,
            memberOptions:
                SymbolDisplayMemberOptions.IncludeExplicitInterface |
                SymbolDisplayMemberOptions.IncludeParameters,
            kindOptions:
                SymbolDisplayKindOptions.None,
            parameterOptions:
                SymbolDisplayParameterOptions.IncludeModifiers |
                SymbolDisplayParameterOptions.IncludeType,
            localOptions:
                SymbolDisplayLocalOptions.IncludeType,
            miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
            );
}