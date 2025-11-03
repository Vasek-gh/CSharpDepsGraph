namespace CSharpDepsGraph.Cli.Commands.Settings;

internal class ExportSettings
{
    public string? OutputPath { get; set; } // todo rename path -> filename

    public bool HideExternal { get; set; }

    public NodeExportLevel ExportLevel { get; set; } // todo nullable and max as default

    public required IEnumerable<RegexSymbolFilter> SymbolFilters { get; init; }

    public static class Defaults
    {
        public static NodeExportLevel ExportLevelFull { get; } = NodeExportLevel.All;

        public static NodeExportLevel ExportLevelOneLevel { get; } = NodeExportLevel.Assembly;
    }
}