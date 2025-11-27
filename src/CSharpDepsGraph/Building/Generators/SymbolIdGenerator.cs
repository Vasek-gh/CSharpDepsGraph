using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CSharpDepsGraph.Building.Generators;

/// <summary>
/// Default symbol identifier generator
/// </summary>
public class SymbolIdGenerator : ISymbolIdGenerator
{
    private uint _counter;
    private readonly ISymbolIdGenerator _generator;

    public SymbolIdGenerator()
        : this(NullLoggerFactory.Instance, false)
    {
    }

    public SymbolIdGenerator(ILoggerFactory loggerFactory, bool disableCache)
    {
        var fullyQualifiedGenerator = new FullyQualifiedIdGenerator(loggerFactory.CreateLogger<FullyQualifiedIdGenerator>(), true);

        _generator = disableCache
            ? fullyQualifiedGenerator
            : new CachedSymbolIdGenerator(loggerFactory.CreateLogger<CachedSymbolIdGenerator>(), fullyQualifiedGenerator);
    }

    /// <inheritdoc/>
    public string Execute(ISymbol symbol)
    {
        return _counter++.ToString();
        //return _generator.Execute(symbol);
    }

    /// <inheritdoc/>
    public void WriteStatistic()
    {
        _generator.WriteStatistic();
    }
}