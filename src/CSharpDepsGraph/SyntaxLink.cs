using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph;

/// <summary>
/// Defines a syntax reference to the source code for a object
/// </summary>
[DebuggerDisplay("{" + nameof(FileKind) + ",nq}:{" + nameof(GetDisplayString) + "(),nq}")]
public sealed class SyntaxLink
{
    /// <summary>
    /// Roslyn syntax node. For external symbols syntax is null
    /// </summary>
    public SyntaxNode? Syntax { get; init; }

    /// <summary>
    /// Location type
    /// </summary>
    public required SyntaxFileKind FileKind { get; init; }

    /// <summary>
    /// Source code file path. For external symbols path equal assembly name
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Source code line number. For external symbols value is zero
    /// </summary>
    public int Line { get; init; }

    /// <summary>
    /// Source code column number. For external symbols value is zero
    /// </summary>
    public int Column { get; init; }

    /// <summary>
    /// Gets a human-readable location string
    /// </summary>
    public string GetDisplayString()
    {
        if (Syntax == null)
        {
            return Path;
        }

        return $"{Path}:{Line}:{Column}";
    }
}