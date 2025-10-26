namespace CSharpDepsGraph.Mutation;

/// <summary>
/// Defines a processor that modifies graph. Implementations should transform the input graph and return a modified version.
/// </summary>
public interface IMutator
{
    /// <summary>
    /// Executes mutation logic
    /// </summary>
    IGraph Run(IGraph graph);
}