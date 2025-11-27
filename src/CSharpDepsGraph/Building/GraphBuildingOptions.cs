namespace CSharpDepsGraph.Building;

public class GraphBuildingOptions
{
    public bool DebugMode { get; set; }

    public bool GenerateFullyQualifiedId { get; set; }

    /// <summary>
    /// Controls whether self-references are emitted as links in the graph.
    /// </summary>
    public bool GenerateLinksToSelfType { get; set; }

    public bool GenerateLinksToPrimitveTypes { get; set; }

    public bool GenerateLinksToTypeQualifier { get; set; }

    public bool GenerateLinksToNamespaceQualifier { get; set; }

    public bool MergeSystemAssemblies { get; set; }

    /// <summary>
    /// Determines whether assemblies with the same name but different versions are merged.
    /// </summary>
    public bool MergeAssembliesWithDifferentVersions { get; set; }

    public HashSet<string> SystemAssemblies { get; set; }
}