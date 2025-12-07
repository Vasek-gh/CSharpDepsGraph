namespace CSharpDepsGraph.Building;

/// <summary>
/// Provides configuration options for building a graph.
/// </summary>
public class GraphBuildOptions
{
    /// <summary>
    /// Default configuration
    /// </summary>
    public static readonly GraphBuildOptions Default = new GraphBuildOptions();

    public bool IncludeLinksToSelfType { get; set; }

    public bool IncludeLinksToPrimitveTypes { get; set; }

    public bool IncludeLinksToTypeQualifier { get; set; } // todo

    public bool IncludeLinksToNamespaceQualifier { get; set; } // todo

    public bool DoNotIgnoreVisibleGeneratedCode { get; set; }

    public bool DoNotMergeAssembliesWithDifferentVersions { get; set; }

    public bool GenerateFullyQualifiedUid { get; set; } = true; // todo kill default value

    public HashSet<string> IgnoreLinksToAssemblies { get; set; } = new HashSet<string>(Utils.CoreLibs);
}