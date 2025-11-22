using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building.Entities;

internal class ExternalNodeSyntaxLink : INodeSyntaxLink
{
    public string Location { get; }

    public LocationKind LocationKind => LocationKind.External;

    public SyntaxNode? Syntax => null;

    public ExternalNodeSyntaxLink(string name)
    {
        Location = name + ".dll";
    }
}