using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph;

/// <summary>
/// Defines a syntax reference to the source code for a object
/// </summary>
public interface INodeSyntaxLink
{
    /// <summary>
    /// Symbol location. For symbols from source it path to file. For external symbols it assembly name
    /// </summary>
    string Location { get; }

    /// <summary>
    /// Location Kind.
    /// </summary>
    LocationKind LocationKind { get; }

    /// <summary>
    /// Reference to syntax. For symbols that do not have sources, null will be set.
    /// </summary>
    SyntaxReference? SyntaxReference { get; }
}