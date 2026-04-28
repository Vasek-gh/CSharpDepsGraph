namespace CSharpDepsGraph.Cli.Commands;

public interface IGraphCommand
{
    Task Execute(GraphContext graphContext, CancellationToken cancellationToken);
}