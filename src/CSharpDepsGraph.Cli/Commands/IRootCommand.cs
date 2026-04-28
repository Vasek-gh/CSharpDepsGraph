namespace CSharpDepsGraph.Cli.Commands;

public interface IRootCommand
{
    Task Execute(CancellationToken cancellationToken);
}