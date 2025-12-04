namespace CSharpDepsGraph.Building;

/// <summary>
/// Provides configuration options for building a graph.
/// </summary>
public class GraphBuildingOptions
{
    /// <summary>
    /// Default configuration
    /// </summary>
    public static readonly GraphBuildingOptions Default = new GraphBuildingOptions();

    public bool IncludeLinksToSelfType { get; set; } // todo

    public bool IncludeLinksToPrimitveTypes { get; set; }// todo

    public bool IncludeLinksToTypeQualifier { get; set; }// todo

    public bool IncludeLinksToNamespaceQualifier { get; set; }// todo

    public bool DoNotIgnoreVisibleGeneratedCode { get; set; }

    public bool DoNotMergeAssembliesWithDifferentVersions { get; set; }// todo

    public bool GenerateFullyQualifiedUid { get; set; } = true; // todo kill default value

    public HashSet<string> IgnoreLinksToAssemblies { get; set; } = new HashSet<string>(Utils.CoreLibs); // todo
}