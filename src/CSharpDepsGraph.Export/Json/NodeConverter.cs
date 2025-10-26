using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Export.Json;

internal class NodeConverter : JsonConverter<INode>
{
    private readonly ILogger _logger;

    public NodeConverter(ILogger logger)
    {
        _logger = logger;
    }

    public override void Write(Utf8JsonWriter writer, INode value, JsonSerializerOptions options)
    {
        _logger.LogTrace($"Write node: {value.Id}...");

        writer.WriteStartObject();

        writer.WritePropertyName(nameof(value.Id));
        writer.WriteStringValue(value.Id);

        writer.WritePropertyName("Type");
        writer.WriteStringValue(value.GetNodeType().ToString());

        writer.WritePropertyName("Caption");
        writer.WriteStringValue(value.GetCaption());

        writer.WritePropertyName("Locations");
        writer.WriteStartArray();
        foreach (var syntaxLink in value.SyntaxLinks)
        {
            writer.WriteStringValue(syntaxLink.GetDisplayString());
        }
        writer.WriteEndArray();

        if (value.Childs.Any())
        {
            writer.WritePropertyName(nameof(value.Childs));
            JsonSerializer.Serialize(writer, value.Childs, options);
        }

        writer.WriteEndObject();
    }

    public override INode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }
}