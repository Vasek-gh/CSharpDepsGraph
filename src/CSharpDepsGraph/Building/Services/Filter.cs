using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building.Services;

internal class Filter : IFilter
{
    private readonly GraphBuildingOptions _options;
    private readonly HashSet<string> _ignoreLinksToAssemblies;

    public Filter(GraphBuildingOptions options)
    {
        _options = options;
        _ignoreLinksToAssemblies = options.IgnoreLinksToAssemblies ?? [];
    }

    public bool FilterLinkTarget(ISymbol source, ISymbol target)
    {
        if (!_options.IncludeLinksToPrimitveTypes && SymbolIsPrimitiveType(target))
        {
            return true;
        }

        if (SymbolIsFromIgnoredAssembly(target))
        {
            return true;
        }

        //Utils.IsPrimiteType()
        return false;
    }

    private bool SymbolIsPrimitiveType(ISymbol symbol)
    {
        return Utils.IsPrimiteType(symbol)
            || (symbol.ContainingType is not null && Utils.IsPrimiteType(symbol.ContainingType));
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