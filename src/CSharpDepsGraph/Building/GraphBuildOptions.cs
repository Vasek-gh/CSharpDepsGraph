namespace CSharpDepsGraph.Building;

/// <summary>
/// Provides configuration options for building a graph.
/// </summary>
public class GraphBuildOptions
{
    public bool IncludeLinksToSelfType { get; set; }

    public bool IncludeLinksToPrimitveTypes { get; set; }

    public bool DoNotIgnoreVisibleGeneratedCode { get; set; }

    public bool DoNotMergeAssembliesWithDifferentVersions { get; set; }

    public bool GenerateFullyQualifiedUid { get; set; }

    public HashSet<string> IgnoreLinksToAssemblies { get; set; } = new HashSet<string>(Utils.CoreLibs);
}