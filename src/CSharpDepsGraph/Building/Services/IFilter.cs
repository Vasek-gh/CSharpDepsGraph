using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building.Services;

internal interface IFilter
{
    bool CanCreateLink(ISymbol source, ISymbol target);
}