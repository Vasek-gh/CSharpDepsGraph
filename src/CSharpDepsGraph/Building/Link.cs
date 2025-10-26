using System.Diagnostics;

namespace CSharpDepsGraph.Building;

[DebuggerDisplay("{Source.Id} -> {Target.Id}")]
internal class Link : ILink
{
    public required INode Source { get; init; }

    public INode OriginalSource => Source;

    public required INode Target { get; init; }

    public INode OriginalTarget => Target;

    public required SyntaxLink SyntaxLink { get; init; }
}