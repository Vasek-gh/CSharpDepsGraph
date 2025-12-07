using System.Diagnostics;
using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Mutation;
using CSharpDepsGraph.Cli.Options;

namespace CSharpDepsGraph.Cli.Commands;

internal static class CommandsUtils
{
    public static Stream CreateOutputStream(string? inputFileName, string? outputFileName, string defaultExtension)
    {
        outputFileName = inputFileName == null
            ? outputFileName ??= $"{nameof(CSharpDepsGraph)}.{defaultExtension}"
            : $"{Path.GetFileNameWithoutExtension(inputFileName)}.{defaultExtension}";

        return new FileStream(outputFileName, FileMode.Create);
    }

    public static IMutator GetFlatExportMutator(ExportOptions settings)
    {
        return new MutatorBuilder()
            .WithExternalHide(settings.HideExternal)
            .WithExportLevel(settings.ExportLevel, true)
            .WithSymbolFilters(settings.SymbolFilters)
            .Build();
    }

    public static IMutator GetHierarchyExportMutator(ExportOptions settings)
    {
        return new MutatorBuilder()
            .WithExternalHide(settings.HideExternal)
            .WithExportLevel(settings.ExportLevel, false)
            .WithSymbolFilters(settings.SymbolFilters)
            .Build();
    }

    public static async Task ExecuteWithReport(ILogger logger, Func<Task> action)
    {
        logger.LogDebug("Begin");

        var sw = new Stopwatch();

        sw.Start();
        await action();
        sw.Stop();

        logger.LogDebug($"Completed in {TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds).TotalSeconds} seconds");
    }
}