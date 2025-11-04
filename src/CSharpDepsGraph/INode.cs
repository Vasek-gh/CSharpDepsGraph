using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph;

/// <summary>
/// Graph node for symbol
/// </summary>
public interface INode
{
    /// <summary>
    /// Unique no identifier
    /// </summary>
    string Id { get; }

    /// <summary>
    /// The symbol for which the node was created
    /// </summary>
    ISymbol? Symbol { get; }

    /// <summary>
    /// Symbols members
    /// </summary>
    IEnumerable<INode> Childs { get; }

    /// <summary>
    /// Links to syntax where the symbol is defined
    /// </summary>
    IEnumerable<INodeSyntaxLink> SyntaxLinks { get; }
}