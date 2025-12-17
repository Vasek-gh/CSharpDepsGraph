namespace CSharpDepsGraph.Cli.Commands;

internal interface IRootCommand
{
    Task Execute(CancellationToken cancellationToken);
}