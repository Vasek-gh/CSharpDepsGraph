namespace CSharpDepsGraph.Export;

/// <summary>
/// Specifies the type of link
/// </summary>
public enum LinkType
{
    /// <summary>
    /// Basic link type
    /// </summary>
    Reference,

    /// <summary>
    /// Set for links between a type and its interface
    /// </summary>
    Implements,

    /// <summary>
    /// Set for links between a type and its base type
    /// </summary>
    Inherits,

    /// <summary>
    /// Set when the symbol makes method calls
    /// </summary>
    Call,
}