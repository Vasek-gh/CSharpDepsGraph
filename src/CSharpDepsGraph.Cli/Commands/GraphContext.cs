namespace CSharpDepsGraph.Cli.Commands;

public sealed class GraphContext
{
    public required IGraph Graph { get; set; }

    public required string InputFile { get; set; }
}
