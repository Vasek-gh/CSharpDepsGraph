namespace CSharpDepsGraph.Cli.Options;

public class ExportOptions
{
    public string? OutputFileName { get; set; }

    public bool HideExternal { get; set; }

    public NodeExportLevel ExportLevel { get; set; } = NodeExportLevel.Default;

    public IEnumerable<NodeFilter> NodeFilters { get; set; } = [];
}