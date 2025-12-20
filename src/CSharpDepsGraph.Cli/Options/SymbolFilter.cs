using CSharpDepsGraph.Transforming.Filtering;

namespace CSharpDepsGraph.Cli.Options;

public class NodeFilter
{
    public required FilterAction FilterAction { get; set; }

    public required string Pattern { get; set; }
}