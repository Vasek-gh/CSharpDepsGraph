namespace CSharpDepsGraph.Building;

public class GraphOptions
{
    public bool GenerateFullyQualifiedId { get; set; }

    public bool GenerateLinksToSelfType { get; set; }

    public bool GenerateLinksToTypeQualifier { get; set; }

    public bool GenerateLinksToNamespaceQualifier { get; set; }

    public bool MergeSystemAssemblies { get; set; }

    public bool MergeAssembliesWithDifferentVersions { get; set; }

    public HashSet<string> SystemAssemblies { get; set; }
}