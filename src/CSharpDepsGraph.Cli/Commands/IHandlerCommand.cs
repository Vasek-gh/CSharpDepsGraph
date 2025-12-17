namespace CSharpDepsGraph.Cli.Commands;

internal interface IHandlerCommand
{
    Task Execute(GraphContext graphContext, CancellationToken cancellationToken);
}