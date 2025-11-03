using System.Collections.Generic;

namespace CSharpDepsGraph.Building;

internal class LinkedSymbolsMap
{
    private readonly Dictionary<string, List<LinkedSymbol>> _items;

    public LinkedSymbolsMap()
    {
        _items = new(10_000);
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
    }

    public List<LinkedSymbol> Get(string symbolId)
    {
        _items.TryGetValue(symbolId, out var result);

        return result ?? [];
    }
}