using CSharpDepsGraph.Building.Entities;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace CSharpDepsGraph.Building;

internal static class Utils
{
    private static readonly Dictionary<string, INodeSyntaxLink> _assemblyLinksCache = new();
    private static readonly Dictionary<string, List<INodeSyntaxLink>> _externalLinksCache = new();

    public static readonly HashSet<string> CoreLibs = [
        "mscorlib",
        "netstandard",
        "System.Runtime"
    ];

    public static ILogger CreateLogger<T>(ILoggerFactory factory, string? entityName)
    {
        var category = entityName is null
            ? typeof(T).Name
            : $"{typeof(T).Name}<{entityName}>";

        return factory.CreateLogger(category);
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

    public static string GetSyntaxLocation(SyntaxNode syntax)
    {
        var span = syntax.SyntaxTree.GetLineSpan(syntax.Span);
        var line = span.StartLinePosition.Line + 1;
        var column = span.StartLinePosition.Character + 1;
        var path = span.Path;

        return $"{path}:{line}:{column}";
    }

    public static bool IsInMetadata(IAssemblySymbol assemblySymbol)
    {
        return assemblySymbol.Locations.Length == 1 && assemblySymbol.Locations[0].IsInMetadata;
    }

    public static T CheckNull<T>([NotNull] T? value, string errorMessage)
    {
        if (value is null)
        {
            throw new Exception(errorMessage);
        }

        return value;
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