using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Export.Json;

internal class LinkConverter : JsonConverter<ILink>
{
    private readonly ILogger _logger;

    public LinkConverter(ILogger logger)
    {
        _logger = logger;
    }

    public override void Write(Utf8JsonWriter writer, ILink value, JsonSerializerOptions options)
    {
        //_logger.LogTrace($"Write link: {value.Source.Id} -> {value.Target.Id}");

        writer.WriteStartObject();

        writer.WritePropertyName(nameof(value.Source));
        writer.WriteStringValue(value.Source.Id);

        writer.WritePropertyName(nameof(value.OriginalSource));
        writer.WriteStringValue(value.OriginalSource.Id);

        writer.WritePropertyName(nameof(value.Target));
        writer.WriteStringValue(value.Target.Id);

        writer.WritePropertyName(nameof(value.OriginalTarget));
        writer.WriteStringValue(value.OriginalTarget.Id);

        writer.WritePropertyName("Type");
        writer.WriteStringValue(value.GetLinkType().ToString());

        writer.WritePropertyName("Location");
        writer.WriteStringValue(value.SyntaxLink.GetDisplayString());

        writer.WriteEndObject();
    }

    public override ILink Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }
}