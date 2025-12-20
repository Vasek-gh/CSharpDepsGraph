using CSharpDepsGraph.Cli.CommandLine;
using CSharpDepsGraph.Cli.Options;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CSharpDepsGraph.Cli.Tests.CommandLine;

internal class GraphVizExportCommandTests : BaseExportCommandTests<ExportOptions>
{
    public GraphVizExportCommandTests()
        : base("graphviz")
    {
    }

    [Test]
    public async Task Defaults()
    {
        await Check("dummy.sln", (b, e) =>
        {
            Assert.That(e.OutputPath, Is.Null);
            Assert.That(e.HideExternal, Is.False);
            Assert.That(e.ExportLevel, Is.EqualTo(NodeExportLevel.Assembly));
            Assert.That(e.NodeFilters, Is.Not.Null);
            Assert.That(e.NodeFilters, Is.Empty);
        });
    }

    [Test]
    public async Task ExportLevel()
    {
        await Check("dummy.sln -el assembly", (b, e) => Assert.That(e.ExportLevel, Is.EqualTo(NodeExportLevel.Assembly)));
        await Check("dummy.sln -el namespace", (b, e) => Assert.That(e.ExportLevel, Is.EqualTo(NodeExportLevel.Namespace)));

        await Check("dummy.sln --export-level assembly", (b, e) => Assert.That(e.ExportLevel, Is.EqualTo(NodeExportLevel.Assembly)));
        await Check("dummy.sln --export-level namespace", (b, e) => Assert.That(e.ExportLevel, Is.EqualTo(NodeExportLevel.Namespace)));

        await CheckError("dummy.sln -el foo", (e) =>
        {
            Assert.That(e, Does.Contain("foo"));
            Assert.That(e, Does.Contain("export-level"));
        });

        await CheckError("dummy.sln -el default", (e) =>
        {
            Assert.That(e, Does.Contain("default"));
            Assert.That(e, Does.Contain("export-level"));
        });

        await CheckError("dummy.sln -el type", (e) =>
        {
            Assert.That(e, Does.Contain("type"));
            Assert.That(e, Does.Contain("export-level"));
        });

        await CheckError("dummy.sln -el public-member", (e) =>
        {
            Assert.That(e, Does.Contain("public-member"));
            Assert.That(e, Does.Contain("export-level"));
        });

        await CheckError("dummy.sln -el all", (e) =>
        {
            Assert.That(e, Does.Contain("all"));
            Assert.That(e, Does.Contain("export-level"));
        });
    }

    protected override async Task Check(string commandArguments, Action<BuildOptions, ExportOptions> validator)
    {
        var mock = Substitute.For<ICommandFactory>();

        BuildOptions? buildOptions = null;
        ExportOptions? exportOptions = null;
        mock.When(x => x.CreateGraphVizExport(Arg.Any<ILoggerFactory>(), Arg.Any<BuildOptions>(), Arg.Any<ExportOptions>()))
            .Do(callInfo =>
            {
                buildOptions = callInfo.ArgAt<BuildOptions>(1);
                exportOptions = callInfo.ArgAt<ExportOptions>(2);
            });

        var result = await Run(commandArguments, mock);

        Assert.That(result, Is.Null);
        Assert.That(buildOptions, Is.Not.Null);
        Assert.That(exportOptions, Is.Not.Null);

        validator(buildOptions, exportOptions);
    }
}
