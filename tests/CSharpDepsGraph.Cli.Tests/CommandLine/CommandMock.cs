using CSharpDepsGraph.Cli.Commands;

namespace CSharpDepsGraph.Cli.Tests.CommandLine;

internal class CommandMock : ICommand
{
    public Task Execute(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
