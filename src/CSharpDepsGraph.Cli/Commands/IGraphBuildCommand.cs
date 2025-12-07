namespace CSharpDepsGraph.Cli.Commands;

internal interface IGraphBuildCommand
{
    Task Execute(CancellationToken cancellationToken);
}