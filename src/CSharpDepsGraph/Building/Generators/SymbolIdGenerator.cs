using Microsoft.CodeAnalysis;
using System.Globalization;

namespace CSharpDepsGraph.Building.Generators;

/// <summary>
/// Default symbol identifier generator
/// </summary>
public class SymbolUidGenerator : ISymbolUidGenerator
{
    private uint _counter;

    public string Execute(ISymbol symbol)
    {
        return _counter++.ToString(CultureInfo.InvariantCulture);
    }

    public static ISymbolUidGenerator Create(GraphBuildingOptions graphOptions)
    {
        return graphOptions.GenerateFullyQualifiedId
            ? new FullyQualifiedIdGenerator()
            : new SymbolUidGenerator();
    }
}