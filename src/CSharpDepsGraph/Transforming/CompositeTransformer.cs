namespace CSharpDepsGraph.Transforming;

/// <summary>
/// A composite transformer that combines several transformers into a series chain.
/// </summary>
public class CompositeTransformer : ITransformer
{
    private readonly IEnumerable<ITransformer> _transformers;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeTransformer"/> class.
    /// </summary>
    public CompositeTransformer(params ITransformer[] transformers)
        : this(transformers as IEnumerable<ITransformer>)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeTransformer"/> class.
    /// </summary>
    public CompositeTransformer(IEnumerable<ITransformer> transformers)
    {
        _transformers = transformers.ToArray();
    }

    /// <inheritdoc/>
    public IGraph Execute(IGraph graph)
    {
        var result = graph;
        foreach (var transformer in _transformers)
        {
            result = transformer.Execute(result);
        }

        return result;
    }
}