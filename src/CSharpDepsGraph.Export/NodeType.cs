namespace CSharpDepsGraph.Export;

/// <summary>
/// Specifies the type of syntax node or group
/// </summary>
public enum NodeType
{
    /// <summary>
    /// Represents unrecognized node type
    /// </summary>
    Unknown,

    /// <summary>
    /// Logical grouping container for related nodes
    /// </summary>
    Group,

    /// <summary>
    /// Assembly definition node
    /// </summary>
    Assembly,

    /// <summary>
    /// Namespace declaration
    /// </summary>
    Namespace,

    /// <summary>
    /// Enum type definition
    /// </summary>
    Enum,

    /// <summary>
    /// Class declaration
    /// </summary>
    Class,

    /// <summary>
    /// Structure declaration
    /// </summary>
    Structure,

    /// <summary>
    /// Record declaration
    /// </summary>
    Record,

    /// <summary>
    /// Interface declaration
    /// </summary>
    Interface,

    /// <summary>
    /// Delegate type declaration
    /// </summary>
    Delegate,

    /// <summary>
    /// Constant declaration
    /// </summary>
    Const,

    /// <summary>
    /// Instance or static field declaration
    /// </summary>
    Field,

    /// <summary>
    /// Property declaration
    /// </summary>
    Property,

    /// <summary>
    /// Event declaration with add/remove handlers
    /// </summary>
    Event,

    /// <summary>
    /// Method declaration (instance, static, or extension)
    /// </summary>
    Method
}