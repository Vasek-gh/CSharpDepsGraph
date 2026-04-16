using CSharpDepsGraph.Cli.Commands;
using CSharpDepsGraph.Cli.Commands.Export;
using CSharpDepsGraph.Cli.Options;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Cli.CommandLine;

internal class CommandFactory : ICommandFactory
{
    public ICommand CreateDgmlExport(ILoggerFactory loggerFactory, BuildOptions buildOptions, ExportOptions exportOptions)
    {
        var command = new DgmlExportCommand(loggerFactory, exportOptions);
        var buildCommand = new BuildCommand(loggerFactory, buildOptions, command);

        return buildCommand;
    }

    public ICommand CreateGraphVizExport(ILoggerFactory loggerFactory, BuildOptions buildOptions, ExportOptions exportOptions)
    {
        var command = new GraphvizExportCommand(loggerFactory, exportOptions);
        var buildCommand = new BuildCommand(loggerFactory, buildOptions, command);

        return buildCommand;
    }

    public ICommand CreateJsonExport(ILoggerFactory loggerFactory, BuildOptions buildOptions, JsonExportOptions exportOptions)
    {
        var command = new JsonExportCommand(loggerFactory, exportOptions);
        var buildCommand = new BuildCommand(loggerFactory, buildOptions, command);

        return buildCommand;
    }
}