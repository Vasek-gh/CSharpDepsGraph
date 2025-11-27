using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Building;

internal class Counters
{
    public int LinkCount { get; set; }
    public int NodeCount { get; set; }
    public int NodeQueryCount { get; set; }
    public int LinkedSymbolCount { get; set; }
    public int LinkedSymbolQueryCount { get; set; }
    public int SyntaxLinkCount { get; set; }
    public int SyntaxLinkQueryCount { get; set; }

    public void Report(ILogger logger)
    {
        if (!logger.IsEnabled(LogLevel.Debug))
        {
            return;
        }

        logger.LogDebug($"Link count: {LinkCount}");
        logger.LogDebug($"Node count: {NodeCount}");
        logger.LogDebug($"Node query count: {NodeQueryCount}");
        logger.LogDebug($"Linked symbol count: {LinkedSymbolCount}");
        logger.LogDebug($"Linked symbol query count: {LinkedSymbolQueryCount}");
        logger.LogDebug($"Syntax link count: {SyntaxLinkCount}");
        logger.LogDebug($"Syntax link query count: {SyntaxLinkQueryCount}");
    }
}