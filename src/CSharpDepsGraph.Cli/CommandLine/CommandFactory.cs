using CSharpDepsGraph.Cli.Commands;
using CSharpDepsGraph.Cli.Options;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Cli.CommandLine;

internal class CommandFactory : ICommandFactory
{
    public IRootCommand CreateDgmlExport(ILoggerFactory loggerFactory, BuildingOptions buildOptions, Options.ExportOptions exportOptions)
    {
        return new BuildCommand(
            loggerFactory,
            buildOptions,
            DgmlExportFactory.CreateGraphCommand(loggerFactory, exportOptions)
            );
    }

    public IRootCommand CreateGraphVizExport(ILoggerFactory loggerFactory, BuildingOptions buildOptions, Options.ExportOptions exportOptions)
    {
        return new BuildCommand(
            loggerFactory,
            buildOptions,
            GraphvizExportFactory.CreateGraphCommand(loggerFactory, exportOptions)
            );
    }

    public IRootCommand CreateJsonExport(ILoggerFactory loggerFactory, BuildingOptions buildOptions, JsonExportOptions exportOptions)
    {
        var basePath = Path.GetDirectoryName(buildOptions.FileName)
            ?? throw new InvalidOperationException($"Invalid path: {buildOptions.FileName}");

        return new BuildCommand(
            loggerFactory,
            buildOptions,
            JsonExportFactory.CreateGraphCommand(loggerFactory, exportOptions, basePath)
            );
    }
}