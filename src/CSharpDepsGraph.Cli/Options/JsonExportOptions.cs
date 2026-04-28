namespace CSharpDepsGraph.Cli.Options;

public class JsonExportOptions : ExportOptions, IOptions
{
    public bool Format { get; set; }
    public bool ExcludeLocations { get; set; }
    public bool InlinePaths { get; set; }

    public override void Verbose(ICollection<KeyValuePair<string, string>> options)
    {
        base.Verbose(options);

        options.AddOptionValue(Format);
        options.AddOptionValue(ExcludeLocations);
        options.AddOptionValue(InlinePaths);
    }
}
