using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Building;

public class Counters
{
    private int _nodeCount;
    private int _linkCount;
    private int _linkedSymbolCount;
    private int _id;
    private int _idAssembly;
    private int _idTypeName;
    private int _idStdTypeName;
    private int _idFromCache;
    private int _idStrLength;

    public void AddNode()
    {
        _nodeCount++;
    }

    public void AddLink()
    {
        _linkCount++;
    }

    public void AddLinkedSymbol()
    {
        _linkedSymbolCount++;
    }

    public void AddId()
    {
        _id++;
    }

    public void AddIdAssembly()
    {
        _idAssembly++;
    }

    public void AddIdTypeName()
    {
        _idTypeName++;
    }

    public void AddIdStdTypeName()
    {
        _idStdTypeName++;
    }

    public void AddIdFromCache()
    {
        _idFromCache++;
    }

    public void AddIdStrLength(int size)
    {
        _idStrLength += size;
    }

    public void Report(ILogger logger)
    {
        var reportStr = $"""
        Report:
            Nodes: {_nodeCount}
            Links: {_linkCount}
            LinkedSymbols: {_linkedSymbolCount}
            Id: {_id}
            IdAssembly: {_idAssembly}
            IdTypeName: {_idTypeName}
            IdStdTypeName: {_idStdTypeName}
            IdFromCache: {_idFromCache}
            IdStrLength: {_idStrLength}
        """;

        logger.LogDebug(reportStr);
    }
}