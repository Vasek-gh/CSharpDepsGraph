using CSharpDepsGraph.Building.Entities;
using System.Collections.Generic;

namespace CSharpDepsGraph.Building;

internal class LinkedSymbolsMap
{
    private readonly Counters _counters;
    private readonly Dictionary<string, List<LinkedSymbol>> _items;

    public LinkedSymbolsMap(Counters counters)
    {
        _items = new(10_000);
        _counters = counters;
    }

    public void Clear()
    {
        _items.Clear();
    }

    public void Add(string symbolId, LinkedSymbol linkedSymbol)
    {
        if (!_items.TryGetValue(symbolId, out var collection))
        {
            collection = new List<LinkedSymbol>();
            _items.Add(symbolId, collection);
        }

        collection.Add(linkedSymbol);
        _counters.AddLinkedSymbol();
    }

    public List<LinkedSymbol> Get(string symbolId)
    {
        _items.TryGetValue(symbolId, out var result);

        return result ?? [];
    }
}