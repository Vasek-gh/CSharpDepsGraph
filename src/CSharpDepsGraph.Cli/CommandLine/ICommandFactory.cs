using CSharpDepsGraph.Cli.Commands;
using CSharpDepsGraph.Cli.Options;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Cli.CommandLine;

public interface ICommandFactory
{
    ICommand CreateDgmlExport(ILoggerFactory loggerFactory, BuildOptions buildOptions, ExportOptions exportOptions);

    ICommand CreateGraphVizExport(ILoggerFactory loggerFactory, BuildOptions buildOptions, ExportOptions exportOptions);

    ICommand CreateJsonExport(ILoggerFactory loggerFactory, BuildOptions buildOptions, JsonExportOptions exportOptions);
}