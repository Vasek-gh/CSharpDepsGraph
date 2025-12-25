using CSharpDepsGraph.Building;
using CSharpDepsGraph.Cli.CommandLine;
using CSharpDepsGraph.Cli.Options;
using CSharpDepsGraph.Transforming.Filtering;
using NSubstitute;
using System.CommandLine.Parsing;

namespace CSharpDepsGraph.Cli.Tests.CommandLine;

public abstract class BaseExportCommandTests<TOptions> where TOptions : ExportOptions
{
    private readonly string _baseCommnad;

    protected BaseExportCommandTests(string baseCommnad)
    {
        _baseCommnad = baseCommnad;

    }

    [Test]
    public async Task BuildDefaults()
    {
        await Check("dummy.sln", (b, e) =>
        {
            Assert.That(b.Properties, Is.Empty);
            Assert.That(b.GraphOptions, Is.Not.Null);
            Assert.That(b.GraphOptions.IncludeLinksToSelfType, Is.False);
            Assert.That(b.GraphOptions.IncludeLinksToPrimitveTypes, Is.False);
            Assert.That(b.GraphOptions.DoNotIgnoreVisibleGeneratedCode, Is.False);
            Assert.That(b.GraphOptions.DoNotMergeAssembliesWithDifferentVersions, Is.False);
            Assert.That(b.GraphOptions.GenerateFullyQualifiedUid, Is.False);
            Assert.That(b.GraphOptions.IgnoreLinksToAssemblies, Is.EquivalentTo(new GraphBuildOptions().IgnoreLinksToAssemblies));
        });
    }

    [Test]
    public async Task FileName()
    {
        await Check("dummy.sln", (b, e) =>
        {
            Assert.That(b.FileName, Is.EqualTo(Path.GetFullPath("dummy.sln")));
        });

        await CheckError("", (e) =>
        {
            Assert.That(e, Does.Contain("argument"));
            Assert.That(e, Does.Contain("missing"));
        });

        await CheckError("dummy1.sln", (e) =>
        {
            Assert.That(e, Does.Contain("file not found").IgnoreCase);
        });
    }

    [Test]
    public async Task Properties()
    {
        await Check("dummy.sln -p key1=value1", (b, e) =>
        {
            Assert.That(b.Properties.Single().Key, Is.EqualTo("key1"));
            Assert.That(b.Properties.Single().Value, Is.EqualTo("value1"));
        });

        await Check("dummy.sln -p key1=\"value1 value2\"", (b, e) =>
        {
            Assert.That(b.Properties.Single().Key, Is.EqualTo("key1"));
            Assert.That(b.Properties.Single().Value, Is.EqualTo("value1 value2"));
        });

        await Check("dummy.sln -p   key1=value1  ", (b, e) =>
        {
            Assert.That(b.Properties.Single().Key, Is.EqualTo("key1"));
            Assert.That(b.Properties.Single().Value, Is.EqualTo("value1"));
        });

        await Check("dummy.sln -p key1=value1 -p key2=value2", (b, e) =>
        {
            Assert.That(b.Properties.Count(), Is.EqualTo(2));
            Assert.That(b.Properties.First().Key, Is.EqualTo("key1"));
            Assert.That(b.Properties.First().Value, Is.EqualTo("value1"));
            Assert.That(b.Properties.Last().Key, Is.EqualTo("key2"));
            Assert.That(b.Properties.Last().Value, Is.EqualTo("value2"));
        });

        await CheckError("dummy.sln -p key1=", (e) =>
        {
            Assert.That(e, Does.Contain("key1"));
            Assert.That(e, Does.Contain("property"));
        });

        await CheckError("dummy.sln -p =value1", (e) =>
        {
            Assert.That(e, Does.Contain("value1"));
            Assert.That(e, Does.Contain("property"));
        });
    }

    [Test]
    public async Task OutputPath()
    {
        var currentDir = Directory.GetCurrentDirectory();

        await Check("dummy.sln -o foo", (b, e) =>
            Assert.That(e.OutputPath, Is.EqualTo(Path.Combine(currentDir, "foo")))
            );
        await Check("dummy.sln -o \"foo bar\"", (b, e) =>
            Assert.That(e.OutputPath, Is.EqualTo(Path.Combine(currentDir, "foo bar")))
        );
        await Check("dummy.sln --output foo", (b, e) =>
            Assert.That(e.OutputPath, Is.EqualTo(Path.Combine(currentDir, "foo")))
            );
        await Check("dummy.sln --output \"foo bar\"", (b, e) =>
            Assert.That(e.OutputPath, Is.EqualTo(Path.Combine(currentDir, "foo bar")))
        );

        await Check("dummy.sln --output C:/foo/bar", (b, e) =>
            Assert.That(e.OutputPath, Is.EqualTo(Path.Combine(currentDir, Path.Combine("C:", "foo", "bar"))))
        );
    }

    [Test]
    public async Task HideExternal()
    {
        await Check("dummy.sln -he", (b, e) => Assert.That(e.HideExternal, Is.True));
        await Check("dummy.sln --hide-external", (b, e) => Assert.That(e.HideExternal, Is.True));
    }

    [Test]
    public async Task NodeFilters()
    {
        await Check("dummy.sln -nf hide,foo*", (b, e) =>
        {
            Assert.That(e.NodeFilters.Single().FilterAction, Is.EqualTo(FilterAction.Hide));
            Assert.That(e.NodeFilters.Single().Pattern, Is.EqualTo("foo*"));
        });

        await Check("dummy.sln -nf hide,\"foo* bar\"", (b, e) =>
        {
            Assert.That(e.NodeFilters.Single().FilterAction, Is.EqualTo(FilterAction.Hide));
            Assert.That(e.NodeFilters.Single().Pattern, Is.EqualTo("foo* bar"));
        });

        await Check("dummy.sln -nf hide,foo* -nf skip,bar*", (b, e) =>
        {
            Assert.That(e.NodeFilters.Count(), Is.EqualTo(2));
            Assert.That(e.NodeFilters.First().FilterAction, Is.EqualTo(FilterAction.Hide));
            Assert.That(e.NodeFilters.First().Pattern, Is.EqualTo("foo*"));
            Assert.That(e.NodeFilters.Last().FilterAction, Is.EqualTo(FilterAction.Skip));
            Assert.That(e.NodeFilters.Last().Pattern, Is.EqualTo("bar*"));
        });

        await Check("dummy.sln --node-filter   hide,foo*   ", (b, e) =>
        {
            Assert.That(e.NodeFilters.Single().FilterAction, Is.EqualTo(FilterAction.Hide));
            Assert.That(e.NodeFilters.Single().Pattern, Is.EqualTo("foo*"));
        });

        await Check("dummy.sln --node-filter   dissolve,foo*   ", (b, e) =>
        {
            Assert.That(e.NodeFilters.Single().FilterAction, Is.EqualTo(FilterAction.Dissolve));
            Assert.That(e.NodeFilters.Single().Pattern, Is.EqualTo("foo*"));
        });

        await Check("dummy.sln --node-filter   skip,foo*   ", (b, e) =>
        {
            Assert.That(e.NodeFilters.Single().FilterAction, Is.EqualTo(FilterAction.Skip));
            Assert.That(e.NodeFilters.Single().Pattern, Is.EqualTo("foo*"));
        });

        await CheckError("dummy.sln -nf -he", (e) =>
        {
            Assert.That(e, Does.Contain("node-filter"));
        });

        await CheckError("dummy.sln -nf bar,foo*", (e) =>
        {
            Assert.That(e, Does.Contain("bar"));
            Assert.That(e, Does.Contain("node-filter"));
        });

        await CheckError("dummy.sln -nf skip,", (e) =>
        {
            Assert.That(e, Does.Contain("skip"));
            Assert.That(e, Does.Contain("node-filter"));
        });

        await CheckError("dummy.sln -nf ,foo*", (e) =>
        {
            Assert.That(e, Does.Contain("foo*"));
            Assert.That(e, Does.Contain("node-filter"));
        });
    }

    protected virtual Task Check(string commandLine, Action<BuildOptions, TOptions> validator)
    {
        return Task.CompletedTask;
    }

    protected async Task CheckError(string commandArguments, Action<string> validator)
    {
        var mock = Substitute.For<ICommandFactory>();
        mock.CreateDgmlExport(default!, default!, default!).Returns(x => new CommandMock());
        mock.CreateGraphVizExport(default!, default!, default!).Returns(x => new CommandMock());
        mock.CreateJsonExport(default!, default!, default!).Returns(x => new CommandMock());

        var result = await Run(commandArguments, mock);

        Assert.That(result, Is.Not.Null);

        validator(result);
    }

    public async Task<string?> Run(string commandArguments, ICommandFactory commandFactory)
    {
        CreateDummySln();

        var args = GetArgs(_baseCommnad + " " + commandArguments);

        var originalStdErrOut = Console.Error;
        try
        {
            using var customWriter = new StringWriter();
            Console.SetError(customWriter);

            var result = await Program.Run(args, commandFactory);
            if (result == 0)
            {
                return null;
            }

            return customWriter.ToString();
        }
        finally
        {
            Console.SetError(originalStdErrOut);
        }
    }

    private static string[] GetArgs(string commandLine)
    {
        return CommandLineParser.SplitCommandLine(commandLine).ToArray();
    }

    private static void CreateDummySln()
    {
        using var stream = new FileStream("dummy.sln", FileMode.Create);
    }
}