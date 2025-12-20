using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Export.Json;

internal class LinkConverter : JsonConverter<ILink>
{
    private readonly ILogger _logger;
    private readonly PathResolver _pathResolver;
    private readonly JsonExportOptions _options;

    public LinkConverter(ILogger logger, PathResolver pathCutter, JsonExportOptions options)
    {
        _logger = logger;
        _pathResolver = pathCutter;
        _options = options;
    }

    public override void Write(Utf8JsonWriter writer, ILink link, JsonSerializerOptions options)
    {
        //_logger.LogTrace($"Write link: {value.Source.Id} -> {value.Target.Id}");

        writer.WriteStartObject();

        writer.WritePropertyName(nameof(link.Source));
        writer.WriteStringValue(link.Source.Uid);

        if (link.Source.Uid != link.OriginalSource.Uid)
        {
            writer.WritePropertyName(nameof(link.OriginalSource));
            writer.WriteStringValue(link.OriginalSource.Uid);
        }

        writer.WritePropertyName(nameof(link.Target));
        writer.WriteStringValue(link.Target.Uid);

        if (link.Target.Uid != link.OriginalTarget.Uid)
        {
            writer.WritePropertyName(nameof(link.OriginalTarget));
            writer.WriteStringValue(link.OriginalTarget.Uid);
        }

        writer.WritePropertyName("Type");
        writer.WriteStringValue(link.GetLinkType().ToString());

        WriteLocation(writer, link);

        writer.WriteEndObject();
    }

    public override ILink Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    private void WriteLocation(Utf8JsonWriter writer, ILink link)
    {
        if (_options.ExcludeLocations)
        {
            return;
        }

        var location = link.SyntaxLink.GetDisplayString((path) => _pathResolver.Handle(link.SyntaxLink));

        writer.WritePropertyName("Location");
        writer.WriteStringValue(location);
    }
}