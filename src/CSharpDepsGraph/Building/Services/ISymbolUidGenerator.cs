using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building.Services;

internal interface ISymbolUidGenerator
{
    string Execute(ISymbol symbol);
}