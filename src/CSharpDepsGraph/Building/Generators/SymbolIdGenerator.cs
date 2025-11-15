using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;

namespace CSharpDepsGraph.Building.Generators;

/// <summary>
/// Default symbol identifier generator
/// </summary>
public class SymbolIdGenerator : ISymbolIdGenerator
{
    private readonly ILogger _logger;
    private readonly ISymbolIdGenerator _generator;

    private readonly bool _legacyValidate;
    private readonly ISymbolIdGenerator _legacyGenerator;
    private readonly List<(string a, string b, ISymbol s)> _legacyDiffItems = new();

    public SymbolIdGenerator()
        : this(NullLoggerFactory.Instance, false, false)
    {
    }

    public SymbolIdGenerator(ILoggerFactory loggerFactory, bool legacyValidate, bool disableCache)
    {
        _logger = loggerFactory.CreateLogger<SymbolIdGenerator>();

        var fullyQualifiedGenerator = new FullyQualifiedIdGenerator(loggerFactory.CreateLogger<FullyQualifiedIdGenerator>(), true);

        _generator = disableCache
            ? fullyQualifiedGenerator
            : new CachedSymbolIdGenerator(loggerFactory.CreateLogger<CachedSymbolIdGenerator>(), fullyQualifiedGenerator);

        _legacyValidate = legacyValidate;
        _legacyDiffItems = new();
        _legacyGenerator = new LegacySymbolIdGenerator(loggerFactory.CreateLogger<LegacySymbolIdGenerator>());
    }

    /// <inheritdoc/>
    public string Execute(ISymbol symbol)
    {
        var result = _generator.Execute(symbol);
        if (_legacyValidate)
        {
            var legacyResult = _legacyGenerator.Execute(symbol);
            if (result != legacyResult)
            {
                _legacyDiffItems.Add((result, legacyResult, symbol));
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public void WriteStatistic()
    {
        _generator.WriteStatistic();
        _legacyGenerator.WriteStatistic();

        foreach (var diffItem in _legacyDiffItems)
        {
            _logger.LogDebug($"\n{diffItem.a}\n{diffItem.b}");
        }
    }
}