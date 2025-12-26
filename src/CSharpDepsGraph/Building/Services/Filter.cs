using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building.Services;

internal class Filter : IFilter
{
    private readonly GraphBuildOptions _options;
    private readonly SymbolComparer _symbolComparer;
    private readonly HashSet<string> _ignoreLinksToAssemblies;

    public Filter(GraphBuildOptions options, SymbolComparer symbolComparer)
    {
        _options = options;
        _symbolComparer = symbolComparer;
        _ignoreLinksToAssemblies = options.IgnoreLinksToAssemblies.ToHashSet();
    }

    public bool FilterLinkTarget(ISymbol source, ISymbol target)
    {
        if (!_options.IncludeLinksToPrimitiveTypes && SymbolIsPrimitiveType(target))
        {
            return true;
        }

        if (!_options.IncludeLinksToSelf && LinkToSelf(source, target))
        {
            return true;
        }

        if (SymbolIsFromIgnoredAssembly(target))
        {
            return true;
        }

        return false;
    }

    private static bool SymbolIsPrimitiveType(ISymbol symbol)
    {
        return Utils.IsPrimiteType(symbol)
            || (symbol.ContainingType is not null && Utils.IsPrimiteType(symbol.ContainingType));
    }

    private bool LinkToSelf(ISymbol source, ISymbol target)
    {
        if (
            _symbolComparer.Compare(source, target, true)
            || _symbolComparer.Compare(source.ContainingType, target, true)
        )
        {
            return true;
        }

        return false;
    }

    private bool SymbolIsFromIgnoredAssembly(ISymbol symbol)
    {
        var assembly = symbol as IAssemblySymbol
            ?? symbol.ContainingAssembly;

        if (assembly is null)
        {
            return false;
        }

        return _ignoreLinksToAssemblies.Contains(assembly.Name);
    }
}