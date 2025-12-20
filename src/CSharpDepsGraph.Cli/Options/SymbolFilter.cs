using CSharpDepsGraph.Transforming.Filtering;

namespace CSharpDepsGraph.Cli.Options;

public class RegexSymbolFilter
{
    public required FilterAction FilterAction { get; set; }

    public required string RegExPattern { get; set; }

    public override string ToString()
    {
        return $"{FilterAction}: {RegExPattern}";
    }
}