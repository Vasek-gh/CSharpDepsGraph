using System.Diagnostics;
using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Transforming;
using CSharpDepsGraph.Cli.Options;

namespace CSharpDepsGraph.Cli.Commands;

public static class CommandsUtils
{
    public static Stream CreateOutputStream(string? inputFileName, string? outputFileName, string defaultExtension)
    {
        outputFileName = inputFileName == null
            ? outputFileName ??= $"{nameof(CSharpDepsGraph)}.{defaultExtension}"
            : $"{Path.GetFileNameWithoutExtension(inputFileName)}.{defaultExtension}";

        return new FileStream(outputFileName, FileMode.Create);
    }

    public static ITransformer GetFlatExportTransformer(ExportOptions settings)
    {
        return new TransformerBuilder()
            .WithExternalHide(settings.HideExternal)
            .WithExportLevel(settings.ExportLevel, true)
            .WithSymbolFilters(settings.NodeFilters)
            .Build();
    }

    public static ITransformer GetHierarchyExportTransformer(ExportOptions settings)
    {
        return new TransformerBuilder()
            .WithExternalHide(settings.HideExternal)
            .WithExportLevel(settings.ExportLevel, false)
            .WithSymbolFilters(settings.NodeFilters)
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