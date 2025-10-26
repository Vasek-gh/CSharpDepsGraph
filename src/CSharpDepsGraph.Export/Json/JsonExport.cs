using System.IO;
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
    private readonly bool _format;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonExport"/> class.
    /// </summary>
    public JsonExport(ILogger<JsonExport> logger, bool format)
    {
        _logger = logger;
        _format = format;

        _options = new JsonSerializerOptions()
        {
            WriteIndented = _format,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Converters = {
                new NodeConverter(_logger),
                new LinkConverter(_logger)
            }
        };
    }

    /// <summary>
    /// todo
    /// </summary>
    public Task Run(IGraph graph, Stream stream, CancellationToken cancellationToken)
    {
        return JsonSerializer.SerializeAsync<IGraph>(stream, graph, _options, cancellationToken);
    }
}