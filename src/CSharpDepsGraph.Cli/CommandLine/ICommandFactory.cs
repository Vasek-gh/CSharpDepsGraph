using CSharpDepsGraph.Building;
using CSharpDepsGraph.Cli.Commands;
using CSharpDepsGraph.Cli.Options;

namespace CSharpDepsGraph.Cli.CommandLine;

internal interface ICommandFactory
{
    IGraphBuildCommand CreateRootCommand(BuildOptions buildOptions, GraphBuildOptions graphBuildOptions, IGraphHandlerCommand command);
    IGraphHandlerCommand CreateGraphCommand(BuildOptions buildOptions, GraphBuildOptions graphBuildOptions);
}