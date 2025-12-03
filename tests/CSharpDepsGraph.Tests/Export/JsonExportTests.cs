using CSharpDepsGraph.Export.Json;
using CSharpDepsGraph.Tests.Syntax;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace CSharpDepsGraph.Tests.Export;

public class JsonExportTests : BaseSyntaxTests
{
    private static readonly string _testProjectPath = TestData.TestProjectPath.Replace("\\", "/");

    [Test]
    public void LocationsDefault()
    {
        var graph = GetGraph(new JsonExportOptions());
        var node = GetNode(graph);

        Assert.That(graph, Is.Not.Null);
        Assert.That(graph.Paths?.Length, Is.GreaterThan(0));
        Assert.That(graph.Paths?[0].StartsWith(_testProjectPath) == true);
        Assert.That(IsIndexedPath(node.Locations?[0]));
        Assert.That(IsIndexedPath(graph.Links[0].Location));
    }

    [Test]
    public void LocationsBasePath()
    {
        var graph1 = GetGraph(new JsonExportOptions() { BasePath = _testProjectPath });
        var graph1Node = GetNode(graph1);

        Assert.That(graph1, Is.Not.Null);
        Assert.That(graph1.Paths?.Length, Is.GreaterThan(0));
        Assert.That(graph1.Paths?[0].StartsWith(_testProjectPath) == false);
        Assert.That(IsIndexedPath(graph1Node.Locations?[0]));
        Assert.That(IsIndexedPath(graph1.Links[0].Location));

        var graph2 = GetGraph(new JsonExportOptions() { BasePath = _testProjectPath, DoNotCreateLocationTable = true });
        var graph2Node = GetNode(graph2);

        Assert.That(graph2, Is.Not.Null);
        Assert.That(graph2.Paths, Is.Null);
        Assert.That(!IsIndexedPath(graph2Node.Locations?[0]));
        Assert.That(!IsIndexedPath(graph2.Links[0].Location));
        Assert.That(graph2Node.Locations?[0].StartsWith(_testProjectPath) == false);
    }

    [Test]
    public void LocationsExcludeLocations()
    {
        var graph = GetGraph(new JsonExportOptions() { ExcludeLocations = true });
        var graphNode = GetNode(graph);

        Assert.That(graph, Is.Not.Null);
        Assert.That(graph.Paths, Is.Null);
        Assert.That(graphNode.Locations, Is.Null);
        Assert.That(graph.Links[0].Location, Is.Null);
    }

    [Test]
    public void AssemblyAttributeLocation()
    {
        var graph = GetGraph(@"
                using System.Runtime.CompilerServices;
                [assembly: InternalsVisibleTo(""Foo.Bar"")]
            ",
            new JsonExportOptions()
            );

        var graphNode = GetNode(graph);

        Assert.That(graph, Is.Not.Null);
        Assert.That(graph.Paths, Is.Null);
        Assert.That(graphNode.Locations, Is.Null);
        Assert.That(graph.Links[0].Location, Is.Null);
    }

    private JsonGraph GetGraph(JsonExportOptions exportOptions)
    {
        return GetGraph(@"
            using System.Threading;
            public class Test {
                public CancellationToken Foo() => CancellationToken.None;
            }",
            exportOptions
        );
    }

    private JsonGraph GetGraph(string source, JsonExportOptions exportOptions)
    {
        var json = GetJson(source, exportOptions);

        return JsonSerializer.Deserialize<JsonGraph>(json)
            ?? throw new InvalidOperationException();
    }

    private static bool IsIndexedPath(string? path)
    {
        return int.TryParse(path?.Split(':')[0], out _);
    }

    private static JsonNode GetNode(JsonGraph graph)
    {
        var result = graph.Root.Childs?[1].Childs?[0].Childs?.SingleOrDefault(n => n.Caption == "Constants");
        Assert.That(result, Is.Not.Null);

        return result;
    }

    private string GetJson(string sourceText, JsonExportOptions exportOptions)
    {
        var graph = Build(sourceText);

        using var stream = new MemoryStream();

        new JsonExport(NullLogger<JsonExport>.Instance, exportOptions).Run(graph, stream, CancellationToken.None);

        stream.Position = 0;
        using var textReader = new StreamReader(stream);
        return textReader.ReadToEnd();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1812")]
    private class JsonGraph
    {
        public required JsonNode Root { get; set; }
        public required JsonLink[] Links { get; set; }
        public required string[]? Paths { get; set; }
    }

    [DebuggerDisplay("{Caption}")]
    private class JsonNode
    {
        public required string Id { get; set; }
        public required string Type { get; set; }
        public required string Caption { get; set; }
        public JsonNode[]? Childs { get; set; }
        public string[]? Locations { get; set; }
    }

    [DebuggerDisplay("{Source} -> {Target}")]
    private class JsonLink
    {
        public required string Source { get; set; }
        public required string Target { get; set; }
        public required string Type { get; set; }
        public string? Location { get; set; }
    }
}