using CSharpDepsGraph.Building;
using CSharpDepsGraph.Cli.Commands;
using CSharpDepsGraph.Cli.Options;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Cli.CommandLine;

internal class ExportOptionsHost<T> where T : class
{
    public required BuildOptions BuildOptions { get; set; }
    public required GraphBuildOptions GraphBuildOptions { get; set; }
    public required T ExportOptions { get; set; }
}

internal interface ICommandFactory
{
    IRootCommand CreateDgmlExportCommand(ILoggerFactory loggerFactory, ExportOptionsHost<ExportOptions> options);

    IRootCommand CreateGraphVizExportCommand(ILoggerFactory loggerFactory, ExportOptionsHost<ExportOptions> options);

    IRootCommand CreateJsonExportCommand(ILoggerFactory loggerFactory, ExportOptionsHost<JsonExportOptions> options);
}