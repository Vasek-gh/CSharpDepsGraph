using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace CSharpDepsGraph.Building;

[DebuggerDisplay("{Source.Id} -> {Target.Id}")]
internal class Link : ILink, ILinkSyntaxLink
{
    public required INode Source { get; init; }

    public INode OriginalSource => Source;

    public required INode Target { get; init; }

    public INode OriginalTarget => Target;

    public ILinkSyntaxLink SyntaxLink => this;

    public required SyntaxNode Syntax { get; init; }

    public required LocationKind LocationKind { get; init; }
}