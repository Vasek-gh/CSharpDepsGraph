namespace CSharpDepsGraph.Cli.Commands;

internal interface IGraphCommand
{
    Task Execute(GraphContext graphContext, CancellationToken cancellationToken);
}