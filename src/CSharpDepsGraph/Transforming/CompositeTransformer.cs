namespace CSharpDepsGraph.Transforming;

/// <summary>
/// A composite transformer that combines several transformers into a series chain.
/// </summary>
public class CompositeTransformer : ITransformer
{
    private readonly IEnumerable<ITransformer> _mutators;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeTransformer"/> class.
    /// </summary>
    public CompositeTransformer(params ITransformer[] mutators)
        : this(mutators as IEnumerable<ITransformer>)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeTransformer"/> class.
    /// </summary>
    public CompositeTransformer(IEnumerable<ITransformer> mutators)
    {
        _mutators = mutators;
    }

    /// <inheritdoc/>
    public IGraph Execute(IGraph graph)
    {
        var result = graph;
        foreach (var mutator in _mutators)
        {
            result = mutator.Execute(result);
        }

        return result;
    }
}