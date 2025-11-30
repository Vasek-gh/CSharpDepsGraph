namespace CSharpDepsGraph.Building;

/// <summary>
/// Provides configuration options for building a graph.
/// </summary>
public class GraphBuildingOptions
{
    /// <summary>
    /// Unique identifiers should be created based on the fully qualified symbol name. This does not affect how the
    /// graph is formed. Can be used for debugging purposes.
    /// </summary>
    public bool GenerateFullyQualifiedUid { get; set; } = true; // todo kill default value

    /// <summary>
    /// Controls whether self-references are emitted as links in the graph.
    /// </summary>
    public bool GenerateLinksToSelfType { get; set; } // todo

    public bool GenerateLinksToPrimitveTypes { get; set; }// todo

    public bool GenerateLinksToTypeQualifier { get; set; }// todo

    public bool GenerateLinksToNamespaceQualifier { get; set; }// todo

    public bool MergeSystemAssemblies { get; set; }// todo

    /// <summary>
    /// Determines whether assemblies with the same name but different versions are merged.
    /// </summary>
    public bool MergeAssembliesWithDifferentVersions { get; set; }// todo

    public HashSet<string> SystemAssemblies { get; set; }// todo
}