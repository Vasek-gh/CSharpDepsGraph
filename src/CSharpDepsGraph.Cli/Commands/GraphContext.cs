namespace CSharpDepsGraph.Cli.Commands;

internal sealed class GraphContext
{
    public required IGraph Graph { get; set; }

    public required string InputFile { get; set; }
}
