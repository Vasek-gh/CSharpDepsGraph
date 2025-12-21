using System;
using System.Collections.Generic;
using System.IO;

namespace CSharpDepsGraph.Export.Json;

internal class PathResolver
{
    private readonly Dictionary<string, int> _pathMap;
    private readonly JsonExportOptions _options;

    public List<string> Paths { get; }

    public PathResolver(JsonExportOptions options)
    {
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
            // todo warning
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