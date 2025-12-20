namespace CSharpDepsGraph.Export.Json;

/// <summary>
/// Options for json export
/// </summary>
public class JsonExportOptions
{
    /// <summary>
    /// Indicate whether the JSON output should be formatted with indentation and line breaks
    /// </summary>
    public bool FormatOutput { get; set; } = true; // todo kill true

    /// <summary>
    /// Indicating whether to exclude source code location from the export
    /// </summary>
    public bool ExcludeLocations { get; set; }

    /// <summary>
    /// Specifies whether a table of locations will be created or whether they will be written directly to the node.
    /// </summary>
    public bool DoNotCreateLocationTable { get; set; }

    /// <summary>
    /// Base path used to convert absolute file paths to relative paths
    /// </summary>
    public string? BasePath { get; set; }
}