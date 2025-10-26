using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Tests.Mocks;

internal class NodeMock : INode
{
    public required string Id { get; set; }

    public ISymbol? Symbol { get; set; }

    public required IEnumerable<INode> Childs { get; set; }

    public required IEnumerable<SyntaxLink> SyntaxLinks { get; set; }
}