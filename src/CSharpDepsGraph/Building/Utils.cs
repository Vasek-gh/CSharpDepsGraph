using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Building;

internal class Utils
{
    private static readonly Dictionary<string, List<SyntaxLink>> _externalLinksCache = new();

    //public static readonly List<> _externalLinksCache = new();

    public static ILogger CreateLogger<T>(ILoggerFactory factory, string entityName)
    {
        return factory.CreateLogger($"{typeof(T).Name}<{entityName}>");
    }

    public static List<SyntaxLink> CreateExternalSyntaxLink(ISymbol symbol)
    {
        return CreateAssemblySyntaxLink(symbol, SyntaxFileKind.External);
    }

    public static List<SyntaxLink> CreateAssemblySyntaxLink(ISymbol symbol, SyntaxFileKind kind)
    {
        var assemblyName = symbol is IAssemblySymbol
            ? symbol.Name
            : symbol.ContainingAssembly.Name;

        if (!_externalLinksCache.TryGetValue(assemblyName, out var links))
        {
            links =
            [
                new SyntaxLink()
                {
                    FileKind = kind,
                    Path = assemblyName + ".dll"
                }
            ];

            _externalLinksCache.Add(assemblyName, links);
        }

        return links;
    }

    public static SyntaxLink CreateSyntaxLink(
        SyntaxNode syntax,
        SyntaxFileKind syntaxFileKind,
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