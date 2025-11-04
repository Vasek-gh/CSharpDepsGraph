using System.Collections.Generic;
using CSharpDepsGraph.Building.Entities;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Building;

internal static class Utils
{
    private static readonly Dictionary<string, INodeSyntaxLink> _assemblyLinksCache = new();
    private static readonly Dictionary<string, List<INodeSyntaxLink>> _externalLinksCache = new();

    public static ILogger CreateLogger<T>(ILoggerFactory factory, string entityName)
    {
        return factory.CreateLogger($"{typeof(T).Name}<{entityName}>");
    }

    public static List<INodeSyntaxLink> CreateExternalSyntaxLink(ISymbol symbol)
    {
        var assemblyName = symbol is IAssemblySymbol
            ? symbol.Name
            : symbol.ContainingAssembly.Name;

        if (!_externalLinksCache.TryGetValue(assemblyName, out var links))
        {
            links = [new ExternalNodeSyntaxLink(assemblyName)];
            _externalLinksCache.Add(assemblyName, links);
        }

        return links;
    }

    public static INodeSyntaxLink CreateAssemblySyntaxLink(string path)
    {
        if (!_assemblyLinksCache.TryGetValue(path, out var link))
        {
            link = new AssemblyNodeSyntaxLink(path);
            _assemblyLinksCache.Add(path, link);
        }

        return link;
    }

    public static SyntaxLink CreateSyntaxLink(
        SyntaxNode syntax,
        LocationKind syntaxFileKind,
        FileLinePositionSpan lineSpan
        )
    {
        return new SyntaxLink()
        {
            FileKind = syntaxFileKind,
            Path = lineSpan.Path,
            Syntax = syntax,
            Line = lineSpan.StartLinePosition.Line + 1,
            Column = lineSpan.StartLinePosition.Character + 1
        };
    }

    public static string GetSyntaxLocation(SyntaxNode syntax)
    {
        var span = syntax.SyntaxTree.GetLineSpan(syntax.Span);
        var line = span.StartLinePosition.Line + 1;
        var column = span.StartLinePosition.Character + 1;
        var path = span.Path;

        return $"{path}:{line}:{column}";
    }

    public static List<T> GetEmptyList<T>()
    {
        return GenericHost<T>.EmptyList;
    }

    private class GenericHost<T>
    {
        public static List<T> EmptyList = [];
    }
}