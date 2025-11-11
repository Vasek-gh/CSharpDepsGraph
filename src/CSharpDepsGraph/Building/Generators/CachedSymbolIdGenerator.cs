using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace CSharpDepsGraph.Building.Generators;

public class CachedSymbolIdGenerator : ISymbolIdGenerator
{
    private readonly ILogger _logger;
    private readonly ISymbolIdGenerator _generator;
    private readonly Dictionary<ISymbol, string> _symbolsCache;

    private int _callCount;
    private int _returnFromCacheCount;

    public CachedSymbolIdGenerator(ILogger<CachedSymbolIdGenerator> logger, ISymbolIdGenerator generator)
    {
        _logger = logger;
        _generator = generator;
        _symbolsCache = new(40_000, SymbolEqualityComparer.Default);
    }

    /// <inheritdoc/>
    public string Execute(ISymbol symbol)
    {
        _callCount++;

        if (_symbolsCache.TryGetValue(symbol, out var cachedId))
        {
            _returnFromCacheCount++;
            return cachedId;
        }

        var result = _generator.Execute(symbol);
        _symbolsCache.Add(symbol, result);

        return result;
    }

    /// <inheritdoc/>
    public void WriteStatistic()
    {
        _generator.WriteStatistic();
        _logger.LogDebug($"Call count: {_callCount}");
        _logger.LogDebug($"From cache count: {_returnFromCacheCount}");
    }
}