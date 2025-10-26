namespace CSharpDepsGraph;

/// <summary>
/// Defines a reference to a symbol used by another symbol
/// </summary>
public interface ILink
{
    /// <summary>
    /// Points to the current source symbol. After mutation, may not point to the original symbol
    /// </summary>
    INode Source { get; }

    /// <summary>
    /// Should point to the original source symbol, even after mutation
    /// </summary>
    INode OriginalSource { get; }

    /// <summary>
    /// Points to the current target symbol. After mutation, may not point to the original symbol
    /// </summary>
    INode Target { get; }

    /// <summary>
    /// Should point to the original target symbol, even after mutation
    /// </summary>
    INode OriginalTarget { get; }

    /// <summary>
    /// Where <see cref="OriginalSource"/> symbol use <see cref="OriginalTarget"/> symbol
    /// </summary>
    SyntaxLink SyntaxLink { get; }
}