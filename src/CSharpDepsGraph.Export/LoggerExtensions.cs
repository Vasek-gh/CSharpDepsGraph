using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Export;

internal static partial class LoggerExtensions
{
    [LoggerMessage(Level = LogLevel.Trace, Message = "Write node: {nodeUid}...")]
    public static partial void LogWriteNode(
        this ILogger logger,
        string nodeUid
        );

    [LoggerMessage(Level = LogLevel.Trace, Message = "Write link: {sourceUid} -> {targetUid}")]
    public static partial void LogWriteLink(
        this ILogger logger,
        string sourceUid,
        string targetUid
        );
}