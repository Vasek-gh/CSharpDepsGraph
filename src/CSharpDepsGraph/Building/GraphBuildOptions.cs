namespace CSharpDepsGraph.Building;

/// <summary>
/// Provides configuration options for building a graph.
/// </summary>
public class GraphBuildOptions
{
    /// <summary>
    /// Uses fully qualified symbol names for node identifiers
    /// </summary>
    public bool FullyQualifiedUid { get; set; }

    /// <summary>
    /// Parse the generated code what not located in intermediate output path
    /// </summary>
    public bool ParseGeneratedCode { get; set; }

    /// <summary>
    /// Include references to symbols from your own type
    /// </summary>
    public bool CreateLinksToSelf { get; set; }

    /// <summary>
    /// Include links to symbols of primitive types
    /// </summary>
    public bool CreateLinksToPrimitiveTypes { get; set; }

    /// <summary>
    /// Сreates separate nodes for each version of an assembly
    /// </summary>
    public bool SplitAssembliesVersions { get; set; }

    /// <summary>
    /// Link to all symbols from the listed assemblies will be ignored
    /// </summary>
    public IEnumerable<string> AssemblyFilter { get; set; } = [];
}