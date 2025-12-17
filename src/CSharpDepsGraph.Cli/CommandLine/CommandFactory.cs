using CSharpDepsGraph.Cli.Commands;
using CSharpDepsGraph.Cli.Commands.Export;
using CSharpDepsGraph.Cli.Options;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Cli.CommandLine;

internal class CommandFactory : ICommandFactory
{
    public IRootCommand CreateDgmlExportCommand(ILoggerFactory loggerFactory, ExportOptionsHost<ExportOptions> options)
    {
        var command = new DgmlExportCommand(loggerFactory, options.ExportOptions);
        var buildCommand = new BuildCommand(loggerFactory, options.BuildOptions, options.GraphBuildOptions, command);

        return buildCommand;
    }

    public IRootCommand CreateGraphVizExportCommand(ILoggerFactory loggerFactory, ExportOptionsHost<ExportOptions> options)
    {
        var command = new GraphvizExportCommand(loggerFactory, options.ExportOptions);
        var buildCommand = new BuildCommand(loggerFactory, options.BuildOptions, options.GraphBuildOptions, command);

        return buildCommand;
    }

    public IRootCommand CreateJsonExportCommand(ILoggerFactory loggerFactory, ExportOptionsHost<JsonExportOptions> options)
    {
        var command = new JsonExportCommand(loggerFactory, options.ExportOptions);
        var buildCommand = new BuildCommand(loggerFactory, options.BuildOptions, options.GraphBuildOptions, command);

        return buildCommand;
    }
}