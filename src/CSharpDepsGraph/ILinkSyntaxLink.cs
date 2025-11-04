using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph;

/// <summary>
/// Defines a syntax reference to the source code where link begin
/// </summary>
public interface ILinkSyntaxLink
{
    /// <summary>
    /// Syntax where <see cref="ILink.OriginalSource"/> symbol use <see cref="ILink.OriginalTarget"/> symbol
    /// </summary>
    SyntaxNode Syntax { get; init; }

    /// <summary>
    /// Location kind of syntax
    /// </summary>
    LocationKind LocationKind { get; init; }
}