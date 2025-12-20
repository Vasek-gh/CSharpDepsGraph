using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CSharpDepsGraph.Building;
using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Tests.Transformation;

internal class NodeMock : INode
{
    private static uint _counter;

    public required string Uid { get; set; }

    public ISymbol? Symbol { get; set; }

    public List<NodeMock> ChildList { get; set; } = [];

    public IEnumerable<INode> Childs => ChildList;

    public List<INodeSyntaxLink> SyntaxLinkList { get; set; } = [];

    public IEnumerable<INodeSyntaxLink> SyntaxLinks => SyntaxLinkList;

    public NodeMock AddNode(string id, ISymbol? symbol = null)
    {
        var node = new NodeMock()
        {
            Uid = id,
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

        var assemblySymbol = Mocks.CreateAssemblySymbol(name, null);

        var node = new NodeMock()
        {
            Uid = GenUid(),
            Symbol = assemblySymbol,
            SyntaxLinkList = [Utils.CreateAssemblySyntaxLink(name)]
        };

        ChildList.Add(node);

        return node;
    }

    public NodeMock AddNamespaceNode(string name)
    {
        var existsNamespace = ChildList.SingleOrDefault(c => c.Symbol is INamedTypeSymbol && c.Symbol.Name == name);
        if (existsNamespace is not null)
        {
            return existsNamespace;
        }

        var namespaceSymbol = Mocks.CreateNamespaceSymbol(name, Symbol);

        var node = new NodeMock()
        {
            Uid = GenUid(),
            Symbol = namespaceSymbol
        };

        ChildList.Add(node);

        return node;
    }

    private static string GenUid()
    {
        return _counter++.ToString(CultureInfo.InvariantCulture);
    }
}