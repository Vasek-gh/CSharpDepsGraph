using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building.Services;

internal class Filter : IFilter
{
    private readonly GraphBuildOptions _options;
    private readonly SymbolComparer _symbolComparer;
    private readonly AssemblyFilter _assemblyFilter;

    public Filter(GraphBuildOptions options, SymbolComparer symbolComparer)
    {
        _options = options;
        _symbolComparer = symbolComparer;
        _assemblyFilter = new AssemblyFilter(options);
    }

    public bool CanCreateLink(ISymbol source, ISymbol target)
    {
        if (!_options.CreateLinksToPrimitiveTypes && SymbolIsPrimitiveType(target))
        {
            return false;
        }

        if (!_options.CreateLinksToSelf && LinkToSelf(source, target))
        {
            return false;
        }

        return SymbolIsFromAllowedAssembly(target);
    }

    private static bool SymbolIsPrimitiveType(ISymbol symbol)
    {
        return Utils.IsPrimitiveType(symbol)
            || (symbol.ContainingType is not null && Utils.IsPrimitiveType(symbol.ContainingType));
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

    private bool SymbolIsFromAllowedAssembly(ISymbol symbol)
    {
        var assembly = symbol as IAssemblySymbol
            ?? symbol.ContainingAssembly;

        if (assembly is null)
        {
            return true;
        }

        return _assemblyFilter.IsAllowed(assembly);
    }
}