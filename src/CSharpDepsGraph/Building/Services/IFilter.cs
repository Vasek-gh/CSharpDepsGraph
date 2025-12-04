using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building.Services;

internal interface IFilter
{
    bool FilterLinkTarget(ISymbol source, ISymbol target);
}