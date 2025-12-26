namespace CSharpDepsGraph.Building;

/// <summary>
/// Provides configuration options for building a graph.
/// </summary>
public class GraphBuildOptions
{
    /// <summary>
    /// Include references to symbols from your own type
    /// </summary>
    public bool IncludeLinksToSelf { get; set; }

    /// <summary>
    /// Include links to symbols of primitive types
    /// </summary>
    public bool IncludeLinksToPrimitiveTypes { get; set; }

    /// <summary>
    /// Parse the visible generated code
    /// </summary>
    public bool ParseVisibleGeneratedCode { get; set; }

    /// <summary>
    /// Сreates separate nodes for each version of an external assembly
    /// </summary>
    public bool SplitAssembliesVersions { get; set; }

    /// <summary>
    /// Uses fully qualified symbol names for node identifiers
    /// </summary>
    public bool GenerateFullyQualifiedUid { get; set; }

    /// <summary>
    /// Remove links to all symbols from the specified assemblies
    /// </summary>
    public IEnumerable<string> IgnoreLinksToAssemblies { get; set; } = Utils.CoreLibs;
}