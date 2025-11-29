namespace CSharpDepsGraph.Export.Json;

public class JsonExportOptions
{
    public bool FormatOutput { get; set; } = true; // todo kill true

    public bool ExcludeLocations { get; set; }

    public bool DoNotCreateLocationTable { get; set; }

    public string? BasePath { get; set; }
}