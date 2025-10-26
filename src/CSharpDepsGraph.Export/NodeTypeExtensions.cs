namespace CSharpDepsGraph.Export;

/// <summary>
/// Extensions for <see cref="NodeType"/>
/// </summary>
public static class NodeTypeExtensions
{
    /// <summary>
    /// Return caption for the node type
    /// </summary>
    public static Color GetColor(this NodeType nodeType)
    {
        return nodeType switch
        {
            NodeType.Group => Color.Group,
            NodeType.Assembly => Color.Assembly,
            NodeType.Namespace => Color.Namespace,
            NodeType.Enum => Color.Enum,
            NodeType.Class => Color.Class,
            NodeType.Structure => Color.Structure,
            NodeType.Interface => Color.Interface,
            NodeType.Const => Color.Const,
            NodeType.Field => Color.Field,
            NodeType.Property => Color.Property,
            NodeType.Method => Color.Method,
            _ => Color.Deafult
        };
    }
}
