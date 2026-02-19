using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Cli.Commands;

internal static partial class LoggerExtensions
{
    [LoggerMessage(Level = LogLevel.Trace, Message = "{operation}: {filePath}")]
    public static partial void LogProjectLoadProgress(
        this ILogger logger,
        ProjectLoadOperation operation,
        string filePath
        );
}