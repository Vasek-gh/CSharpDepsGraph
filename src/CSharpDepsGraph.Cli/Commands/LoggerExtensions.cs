using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Cli.Commands;

internal static partial class LoggerExtensions
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "{operation}: {filePath}", EventId = 0)]
    public static partial void LogProjectLoadProgress(
        this ILogger logger,
        ProjectLoadOperation operation,
        string filePath
        );
}