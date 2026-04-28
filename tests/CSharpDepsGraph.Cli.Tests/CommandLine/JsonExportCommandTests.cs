using CSharpDepsGraph.Cli.CommandLine;
using CSharpDepsGraph.Cli.Options;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CSharpDepsGraph.Cli.Tests.CommandLine;

internal class JsonExportCommandTests : BaseExportCommandTests<JsonExportOptions>
{
    public JsonExportCommandTests()
        : base("json")
    {
    }

    [Test]
    public async Task Defaults()
    {
        await Check("dummy.sln", (b, e) =>
        {
            Assert.That(e.OutputFileName, Is.Null);
            Assert.That(e.HideExternal, Is.False);
            Assert.That(e.ExportLevel, Is.EqualTo(NodeExportLevel.All));
            Assert.That(e.NodeFilters, Is.Not.Null);
            Assert.That(e.NodeFilters, Is.Empty);
            Assert.That(e.Format, Is.False);
        });
    }

    [Test]
    public async Task ExportLevel()
    {
        await Check("dummy.sln --export-level assembly", (b, e) => Assert.That(e.ExportLevel, Is.EqualTo(NodeExportLevel.Assembly)));
        await Check("dummy.sln --export-level namespace", (b, e) => Assert.That(e.ExportLevel, Is.EqualTo(NodeExportLevel.Namespace)));
        await Check("dummy.sln --export-level type", (b, e) => Assert.That(e.ExportLevel, Is.EqualTo(NodeExportLevel.Type)));
        await Check("dummy.sln --export-level public-member", (b, e) => Assert.That(e.ExportLevel, Is.EqualTo(NodeExportLevel.PublicMember)));
        await Check("dummy.sln --export-level all", (b, e) => Assert.That(e.ExportLevel, Is.EqualTo(NodeExportLevel.All)));

        await CheckError("dummy.sln --export-level default", (e) =>
        {
            Assert.That(e, Does.Contain("default"));
            Assert.That(e, Does.Contain("export-level"));
        });

        await CheckError("dummy.sln --export-level foo", (e) =>
        {
            Assert.That(e, Does.Contain("foo"));
            Assert.That(e, Does.Contain("export-level"));
        });
    }

    [Test]
    public async Task FormatFlag()
    {
        await Check("dummy.sln --format", (b, e) =>
        {
            Assert.That(e.Format, Is.True);
        });
    }

    protected override async Task Check(string commandLine, Action<BuildingOptions, JsonExportOptions> validator)
    {
        var mock = Substitute.For<ICommandFactory>();

        BuildingOptions? buildOptions = null;
        JsonExportOptions? exportOptions = null;
        mock.When(x => x.CreateJsonExport(Arg.Any<ILoggerFactory>(), Arg.Any<BuildingOptions>(), Arg.Any<JsonExportOptions>()))
            .Do(callInfo =>
            {
                buildOptions = callInfo.ArgAt<BuildingOptions>(1);
                exportOptions = callInfo.ArgAt<JsonExportOptions>(2);
            });

        var result = await Run(commandLine, mock);

        Assert.That(result, Is.Null);
        Assert.That(buildOptions, Is.Not.Null);
        Assert.That(exportOptions, Is.Not.Null);

        validator(buildOptions, exportOptions);
    }
}
