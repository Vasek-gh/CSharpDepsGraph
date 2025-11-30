namespace CSharpDepsGraph;

/// <summary>
/// Gives a definition of the symbol location type
/// </summary>
public enum LocationKind
{
    /// <summary>
    /// The symbol was taken from the source code
    /// </summary>
    Regular,

    /// <summary>
    /// The symbol was taken from the generated code
    /// </summary>
    Generated,

    /// <summary>
    /// The symbol was taken from the external project or library
    /// </summary>
    External
}