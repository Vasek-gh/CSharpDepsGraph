using Microsoft.CodeAnalysis;
using Microsoft.Extensions.FileSystemGlobbing;

namespace CSharpDepsGraph.Building.Services;

internal class AssemblyFilter
{
    private static readonly Dictionary<string, string[]> _predifinedFilters = new(StringComparer.OrdinalIgnoreCase)
    {
        { "all", ["*"] },
        { "std-lib", Utils.CoreLibs.ToArray() },
        { "sys", ["System*"] },
        { "ms", ["Microsoft*"] },
        { "ms-extensons", ["Microsoft.Extensions*"] },
    };

    private readonly Matcher _matcher;

    public AssemblyFilter(GraphBuildOptions options)
    {
        _matcher = new();
        foreach (var pattern in GetPatterns(options))
        {
            _matcher.AddInclude(pattern);
        }
    }

    public bool IsAllowed(IAssemblySymbol assemblySymbol)
    {
        if (!assemblySymbol.IsFromMetadata())
        {
            return true;
        }

        var matchingResult = _matcher.Match(".", assemblySymbol.Name);
        return !matchingResult.HasMatches;
    }

    private static List<string> GetPatterns(GraphBuildOptions options)
    {
        var items = options.AssemblyFilter
            .Select(af => af.Trim())
            .Where(af => !string.IsNullOrWhiteSpace(af))
            .ToHashSet();

        var result = new List<string>();

        foreach (var item in items)
        {
            if (!item.StartsWith('<') || !item.EndsWith('>'))
            {
                result.Add(item);
                continue;
            }

            var specialItem = item.Substring(1, item.Length - 2);
            if (_predifinedFilters.TryGetValue(specialItem, out var specialItems))
            {
                result.AddRange(specialItems);
            }
        }

        if (result.Any(i => i == "*"))
        {
            result.Clear();
            result.Add("*");
        }

        return result;
    }
}