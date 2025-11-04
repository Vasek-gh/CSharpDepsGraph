using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building.Entities;

internal class AssemblyNodeSyntaxLink : INodeSyntaxLink
{
    public string Location { get; }

    public LocationKind LocationKind => LocationKind.Local;

    public SyntaxReference? SyntaxReference => null;

    public AssemblyNodeSyntaxLink(string path)
    {
        Location = path;
    }

    public bool IsSame(string path)
    {
        return Location == path;
    }
}