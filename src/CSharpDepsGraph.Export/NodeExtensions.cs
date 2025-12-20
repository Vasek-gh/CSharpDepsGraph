namespace CSharpDepsGraph.Export;

/// <summary>
/// Extensions for <see cref="INode"/>
/// </summary>
public static class NodeExtensions
{
    /// <summary>
    /// Return caption for the node
    /// </summary>
    public static string GetCaption(this INode node)
    {
        return node.Symbol is null
            ? node.Uid
            : SymbolExtensions.GetCaption(node.Symbol) ?? node.Uid;
    }

    /// <summary>
    /// Return node type for the node
    /// </summary>
    public static NodeType GetNodeType(this INode node)
    {
        return node.Symbol is null
            ? NodeType.Group
            : SymbolExtensions.GetNodeType(node.Symbol) ?? NodeType.Unknown;
    }
}