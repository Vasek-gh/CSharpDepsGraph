using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building.Entities;

internal class NodeSyntaxLink : INodeSyntaxLink
{
    public string Location => SyntaxReference.SyntaxTree.FilePath;

    public LocationKind LocationKind { get; }

    public SyntaxReference SyntaxReference { get; }

    public NodeSyntaxLink(LocationKind locationKind, SyntaxReference syntaxReference)
    {
        LocationKind = locationKind;
        SyntaxReference = syntaxReference;
    }

    public bool IsSame(LocationKind locationKind, SyntaxReference syntaxReference)
    {
        return LocationKind == locationKind
            && SyntaxReference.SyntaxTree.FilePath == syntaxReference.SyntaxTree.FilePath
            && SyntaxReference.Span == syntaxReference.Span;
    }
}