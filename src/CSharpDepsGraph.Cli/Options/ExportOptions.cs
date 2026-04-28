namespace CSharpDepsGraph.Cli.Options;

public class ExportOptions : IOptions
{
    public string? OutputFileName { get; set; }

    public bool HideExternal { get; set; }

    public NodeExportLevel ExportLevel { get; set; } = NodeExportLevel.Default;

    public IEnumerable<NodeFilter> NodeFilters { get; set; } = [];

    public virtual void Verbose(ICollection<KeyValuePair<string, string>> options)
    {
        options.AddOptionValue(OutputFileName);
        options.AddOptionValue(HideExternal);
        options.AddOptionValue(ExportLevel);
        options.AddOptionValue(NodeFilters);
    }
}