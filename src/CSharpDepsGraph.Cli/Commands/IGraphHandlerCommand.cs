namespace CSharpDepsGraph.Cli.Commands;

internal interface IGraphHandlerCommand
{
    Task Execute(GraphContext graphContext, CancellationToken cancellationToken);
}