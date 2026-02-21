using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Export.Json;

internal class PathResolver
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, int> _pathMap;
    private readonly JsonExportOptions _options;

    public List<string> Paths { get; }

    public PathResolver(ILogger logger, JsonExportOptions options)
    {
        _logger = logger;
        _options = options;

        _pathMap = new();
        Paths = new();
    }

    public string Handle(INodeSyntaxLink syntaxLink)
    {
        if (syntaxLink.LocationKind == LocationKind.External)
        {
            return syntaxLink.Location;
        }

        return Handle(syntaxLink.Location);
    }

    public string Handle(ILinkSyntaxLink syntaxLink)
    {
        if (syntaxLink.LocationKind == LocationKind.External)
        {
            _logger.LogWarning($"Unexpected location kind: {syntaxLink.Syntax.SyntaxTree.FilePath}");
        }

        return Handle(syntaxLink.Syntax.SyntaxTree.FilePath);
    }

    private string Handle(string path)
    {
        if (_options.InlinePaths)
        {
            return HandlePath(path);
        }

        var originalPath = path;
        if (!_pathMap.TryGetValue(originalPath, out var pathIndex))
        {
            path = HandlePath(path);
            pathIndex = Paths.Count;

            _pathMap.Add(originalPath, pathIndex);
            Paths.Add(path);
        }

        return pathIndex.ToString(null, null);
    }

    private string HandlePath(string path)
    {
        if (_options.BasePath is not null)
        {
            path = Path.GetRelativePath(_options.BasePath, path);
        }

        return path.Replace("\\", "/", StringComparison.Ordinal);
    }
}