using CSharpDepsGraph.Mutation.Filtering;

namespace CSharpDepsGraph.Cli.Options;

internal class RegexSymbolFilter
{
    public required FilterAction FilterAction { get; set; }

    public required string RegExPattern { get; set; }

    public override string ToString()
    {
        return $"{FilterAction}: {RegExPattern}";
    }
}