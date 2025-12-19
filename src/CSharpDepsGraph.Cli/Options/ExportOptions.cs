namespace CSharpDepsGraph.Cli.Options;

public class ExportOptions
{
    public string? OutputPath { get; set; } // todo rename path -> filename

    public bool HideExternal { get; set; }

    public NodeExportLevel ExportLevel { get; set; } = NodeExportLevel.Default;

    public IEnumerable<RegexSymbolFilter> SymbolFilters { get; set; } = [];
}