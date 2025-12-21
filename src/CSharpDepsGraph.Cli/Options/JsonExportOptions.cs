namespace CSharpDepsGraph.Cli.Options;

public class JsonExportOptions : ExportOptions
{
    public bool Format { get; set; }
    public bool ExcludeLocations { get; set; }
    public bool InlinePaths { get; set; }
}
