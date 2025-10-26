namespace CSharpDepsGraph;

/// <summary>
/// Gives a definition of the symbol location type
/// </summary>
public enum SyntaxFileKind
{
    /// <summary>
    /// The symbol was taken from the source code
    /// </summary>
    Local,

    /// <summary>
    /// The symbol was taken from the external project or library
    /// </summary>
    External,

    /// <summary>
    /// The symbol was taken from the generated code
    /// </summary>
    Generated
}