namespace CSharpDepsGraph.Transforming;

/// <summary>
/// Defines a processor that modifies graph. Implementations should transform the input graph and return a modified version.
/// </summary>
public interface ITransformer
{
    /// <summary>
    /// Executes mutation logic
    /// </summary>
    IGraph Execute(IGraph graph);
}