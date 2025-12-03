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

    public void Prepare(IGraph graph)
    {
        graph.Root.VisitNodes((n) =>
        {
            foreach (var syntaxLink in n.SyntaxLinks)
            {
                Handle(syntaxLink, true);
            }

            return true;
        });
    }

    public string Handle(INodeSyntaxLink syntaxLink)
    {
        return Handle(syntaxLink, false);
    }

    private string Handle(INodeSyntaxLink syntaxLink, bool preparation)
    {
        if (syntaxLink.Location.EndsWith("AssemblyInfo.cs"))
        {
            // todo kill
        }

        if (syntaxLink.LocationKind == LocationKind.External)
        {
            return syntaxLink.Location;
        }

        return Handle(syntaxLink.Location, preparation);
    }

    public string Handle(ILinkSyntaxLink syntaxLink)
    {
        if (syntaxLink.LocationKind == LocationKind.External)
        {
            // todo warnig
            return "";
        }

        return Handle(syntaxLink.Syntax.SyntaxTree.FilePath, false);
    }

    private string Handle(string path, bool preparation)
    {
        if (_options.DoNotCreateLocationTable)
        {
            return HandlePath(path);
        }

        var originalPath = path;
        if (!_pathMap.TryGetValue(originalPath, out var pathIndex))
        {
            if (!preparation)
            {
                // todo warning. Prepare must fill it
            }

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