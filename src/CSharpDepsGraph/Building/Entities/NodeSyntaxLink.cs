using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building.Entities;

internal class NodeSyntaxLink : INodeSyntaxLink
{
    public string Location => Syntax.SyntaxTree.FilePath;

    public LocationKind LocationKind { get; }

    public SyntaxNode Syntax { get; }

    public NodeSyntaxLink(LocationKind locationKind, SyntaxNode syntax)
    {
        LocationKind = locationKind;
        Syntax = syntax;
    }

    public bool IsSame(LocationKind locationKind, SyntaxNode syntax)
    {
        return LocationKind == locationKind
            && Syntax.Span == syntax.Span
            && Syntax.SyntaxTree.FilePath == syntax.SyntaxTree.FilePath;
    }
}