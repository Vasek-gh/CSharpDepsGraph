using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Export.Json;

internal class NodeConverter : JsonConverter<INode>
{
    private readonly ILogger _logger;
    private readonly PathResolver _pathResolver;
    private readonly JsonExportOptions _options;

    public NodeConverter(ILogger logger, PathResolver pathResolver, JsonExportOptions options)
    {
        _logger = logger;
        _pathResolver = pathResolver;
        _options = options;
    }

    public override void Write(Utf8JsonWriter writer, INode node, JsonSerializerOptions options)
    {
        //_logger.LogTrace($"Write node: {value.Id}...");

        writer.WriteStartObject();

        writer.WritePropertyName(nameof(node.Id));
        writer.WriteStringValue(node.Id);

        writer.WritePropertyName("Type");
        writer.WriteStringValue(node.GetNodeType().ToString());

        writer.WritePropertyName("Caption");
        writer.WriteStringValue(node.GetCaption());

        WriteLocations(writer, node);
        WriteChilds(writer, node, options);

        writer.WriteEndObject();
    }

    public override INode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    private void WriteLocations(Utf8JsonWriter writer, INode node)
    {
        if (_options.ExcludeLocations || !node.SyntaxLinks.Any())
        {
            return;
        }

        writer.WritePropertyName("Locations");
        writer.WriteStartArray();
        foreach (var syntaxLink in node.SyntaxLinks)
        {
            var location = syntaxLink.GetDisplayString((path) => _pathResolver.Handle(syntaxLink));
            writer.WriteStringValue(location);
        }
        writer.WriteEndArray();
    }

    private static void WriteChilds(Utf8JsonWriter writer, INode node, JsonSerializerOptions options)
    {
        if (!node.Childs.Any())
        {
            return;
        }

        writer.WritePropertyName(nameof(node.Childs));
        JsonSerializer.Serialize(writer, node.Childs, options);
    }
}