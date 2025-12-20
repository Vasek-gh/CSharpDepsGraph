namespace CSharpDepsGraph.Transforming;

/// <summary>
/// todo
/// </summary>
public class CompositeMutator : ITransformer
{
    private readonly IEnumerable<ITransformer> _mutators;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeMutator"/> class.
    /// </summary>
    public CompositeMutator(params ITransformer[] mutators)
        : this(mutators as IEnumerable<ITransformer>)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeMutator"/> class.
    /// </summary>
    public CompositeMutator(IEnumerable<ITransformer> mutators)
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