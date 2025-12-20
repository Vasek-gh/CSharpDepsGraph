namespace CSharpDepsGraph.Cli.Commands;

public interface IHandlerCommand
{
    Task Execute(GraphContext graphContext, CancellationToken cancellationToken);
}