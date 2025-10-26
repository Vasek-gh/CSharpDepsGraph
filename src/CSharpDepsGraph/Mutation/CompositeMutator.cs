using System.Collections.Generic;

namespace CSharpDepsGraph.Mutation;

/// <summary>
/// todo
/// </summary>
public class CompositeMutator : IMutator
{
    private readonly IEnumerable<IMutator> _mutators;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeMutator"/> class.
    /// </summary>
    public CompositeMutator(params IMutator[] mutators)
        : this(mutators as IEnumerable<IMutator>)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeMutator"/> class.
    /// </summary>
    public CompositeMutator(IEnumerable<IMutator> mutators)
    {
        _mutators = mutators;
    }

    /// <inheritdoc/>
    public IGraph Run(IGraph graph)
    {
        var result = graph;
        foreach (var mutator in _mutators)
        {
            result = mutator.Run(result);
        }

        return result;
    }
}