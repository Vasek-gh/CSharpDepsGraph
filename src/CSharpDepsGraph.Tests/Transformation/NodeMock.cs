using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Tests.Transformation;

internal class NodeMock : INode
{
    public required string Id { get; set; }

    public ISymbol? Symbol { get; set; }

    public List<NodeMock> ChildList { get; set; } = [];

    public IEnumerable<INode> Childs => ChildList;

    public List<INodeSyntaxLink> SyntaxLinkList { get; set; } = [];

    public IEnumerable<INodeSyntaxLink> SyntaxLinks => SyntaxLinkList;

    public NodeMock AddNode(string id, ISymbol? symbol = null)
    {
        var node = new NodeMock()
        {
            Id = id,
            Symbol = symbol,
        };

        ChildList.Add(node);

        return node;
    }

    public NodeMock AddAssemblyNode(string name)
    {
        var existsAssembly = ChildList.SingleOrDefault(c => c.Symbol is IAssemblySymbol && c.Symbol.Name == name);
        if (existsAssembly is not null)
        {
            return existsAssembly;
        }

        return Mocks.CreateAssemblyNode(name, this);
    }

    public NodeMock AddNamespaceNode(string name)
    {
        var existsNamespace = ChildList.SingleOrDefault(c => c.Symbol is INamedTypeSymbol && c.Symbol.Name == name);
        if (existsNamespace is not null)
        {
            return existsNamespace;
        }

        return Mocks.CreateNamespaceNode(name, this);
    }
}