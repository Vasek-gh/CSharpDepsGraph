namespace CSharpDepsGraph.Transforming;

/// <summary>
/// Defines a transformer that modifies graph. Implementations should transform the input graph and return a modified version.
/// </summary>
public interface ITransformer
{
    /// <summary>
    /// Executes transforming
    /// </summary>
    IGraph Execute(IGraph graph);
}