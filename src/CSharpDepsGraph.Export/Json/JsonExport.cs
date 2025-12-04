using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Export.Json;

/// <summary>
/// todo
/// </summary>
public class JsonExport
{
    private readonly ILogger _logger;
    private readonly PathResolver _pathResolver;
    private readonly JsonExportOptions _exportOptions;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonExport"/> class.
    /// </summary>
    public JsonExport(ILogger<JsonExport> logger, JsonExportOptions exportOptions)
    {
        _logger = logger;
        _exportOptions = exportOptions;

        _pathResolver = new(exportOptions);

        _jsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = exportOptions.FormatOutput,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = {
                new NodeConverter(_logger, _pathResolver, exportOptions),
                new LinkConverter(_logger, _pathResolver, exportOptions),
            }
        };
    }

    /// <summary>
    /// todo
    /// </summary>
    public Task Run(IGraph graph, Stream stream, CancellationToken cancellationToken)
    {
        var graphProxy = new Graph(_pathResolver, _exportOptions)
        {
            Root = graph.Root,
            Links = graph.Links,
        };

        return JsonSerializer.SerializeAsync<Graph>(stream, graphProxy, _jsonOptions, cancellationToken);
    }

    // todo For some reason, the implementation via GraphConverter consumes much more memory.
    private class Graph
    {
        private readonly PathResolver _pathResolver;
        private readonly JsonExportOptions _options;

        public required INode Root { get; set; }
        public required IEnumerable<ILink> Links { get; set; }
        public List<string>? Paths => CanExportPaths() ? _pathResolver.Paths : null;

        public Graph(PathResolver pathResolver, JsonExportOptions options)
        {
            _pathResolver = pathResolver;
            _options = options;
        }

        private bool CanExportPaths()
        {
            return !_options.ExcludeLocations
                && !_options.DoNotCreateLocationTable
                && _pathResolver.Paths.Count > 0;
        }
    }
}