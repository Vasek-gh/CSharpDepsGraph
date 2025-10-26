using System.Diagnostics;
using Microsoft.Extensions.Logging;
using CSharpDepsGraph.Cli.Commands.Settings;
using CSharpDepsGraph.Mutation;

namespace CSharpDepsGraph.Cli.Commands;

internal static class Utils
{
    public static string? GetFileNameError(string fileName)
    {
        var extension = Path.GetExtension(fileName);

        if (extension != ".sln" && extension != ".csproj")
        {
            return $"Unsupported file type: {fileName}";
        }

        if (!File.Exists(fileName))
        {
            return $"File not found: {fileName}";
        }

        return null;
    }

    public static Stream CreateOutputStream(string? inputFileName, string? outputFileName, string defaultExtension)
    {
        outputFileName = inputFileName == null
            ? outputFileName ??= $"{nameof(CSharpDepsGraph)}.{defaultExtension}"
            : $"{Path.GetFileNameWithoutExtension(inputFileName)}.{defaultExtension}";

        return new FileStream(outputFileName, FileMode.Create);
    }

    public static IMutator GetFlatExportMutator(ExportSettings settings)
    {
        return new MutatorBuilder()
            .WithExternalHide(settings.HideExternal)
            .WithExportLevel(settings.ExportLevel, true)
            .WithSymbolFilters(settings.SymbolFilters)
            .Build();
    }

    public static IMutator GetHierarchyExportMutator(ExportSettings settings)
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