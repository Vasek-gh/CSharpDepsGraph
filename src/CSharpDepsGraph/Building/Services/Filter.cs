using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building.Services;

internal class Filter : IFilter
{
    private readonly GraphBuildingOptions _options;

    public Filter(GraphBuildingOptions options)
    {
        _options = options;
    }

    public bool FilterLinkTarget(ISymbol source, ISymbol target)
    {
        if (!_options.IncludeLinksToPrimitveTypes && SymbolIsPrimitiveType(target))
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
}