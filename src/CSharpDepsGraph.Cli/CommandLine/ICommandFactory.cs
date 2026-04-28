using CSharpDepsGraph.Cli.Commands;
using CSharpDepsGraph.Cli.Options;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Cli.CommandLine;

public interface ICommandFactory
{
    IRootCommand CreateDgmlExport(ILoggerFactory loggerFactory, BuildingOptions buildOptions, Options.ExportOptions exportOptions);

    IRootCommand CreateGraphVizExport(ILoggerFactory loggerFactory, BuildingOptions buildOptions, Options.ExportOptions exportOptions);

    IRootCommand CreateJsonExport(ILoggerFactory loggerFactory, BuildingOptions buildOptions, JsonExportOptions exportOptions);
}