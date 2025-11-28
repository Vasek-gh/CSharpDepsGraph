using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building;

internal interface ISymbolUidGenerator
{
    string Execute(ISymbol symbol);
}