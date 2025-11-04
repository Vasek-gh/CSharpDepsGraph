using System.Diagnostics;

namespace CSharpDepsGraph.Mutation;

[DebuggerDisplay("{Source.Id} -> {Target.Id}")]
internal class MutatedLink : ILink
{
    public required INode Source { get; init; }

    public INode OriginalSource { get; }

    public required INode Target { get; init; }

    public INode OriginalTarget { get; }

    public required ILinkSyntaxLink SyntaxLink { get; init; }

    private MutatedLink(ILink src)
    {
        OriginalSource = src.OriginalSource;
        OriginalTarget = src.OriginalTarget;
        SyntaxLink = src.SyntaxLink;
    }

    public static MutatedLink Copy(ILink link, INode source, INode target)
    {
        return new MutatedLink(link)
        {
            Source = source,
            Target = target,
            SyntaxLink = link.SyntaxLink,
        };
    }
}