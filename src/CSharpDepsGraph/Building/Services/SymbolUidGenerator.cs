using Microsoft.CodeAnalysis;
using System.Globalization;

namespace CSharpDepsGraph.Building.Services;

/// <summary>
/// Default symbol identifier generator
/// </summary>
internal class SymbolUidGenerator : ISymbolUidGenerator
{
    private uint _counter;

    public string Execute(ISymbol symbol)
    {
        return _counter++.ToString(CultureInfo.InvariantCulture);
    }

    public static ISymbolUidGenerator Create(GraphBuildingOptions graphOptions)
    {
        return graphOptions.GenerateFullyQualifiedUid
            ? new FullyQualifiedUidGenerator()
            : new SymbolUidGenerator();
    }
}