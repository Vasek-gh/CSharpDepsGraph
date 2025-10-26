namespace CSharpDepsGraph.Cli.Commands.Settings;

internal class BuildSettings
{
    public required string FileName { get; set; }

    public string? Configuration { get; set; }

    public required IEnumerable<KeyValuePair<string, string>> Properties { get; set; }
}