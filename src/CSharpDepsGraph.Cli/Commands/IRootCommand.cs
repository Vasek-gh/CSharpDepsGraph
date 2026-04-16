namespace CSharpDepsGraph.Cli.Commands;

public interface ICommand
{
    Task Execute(CancellationToken cancellationToken);
}