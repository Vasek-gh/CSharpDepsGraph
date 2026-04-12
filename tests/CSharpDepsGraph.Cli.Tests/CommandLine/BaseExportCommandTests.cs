using CSharpDepsGraph.Building;
using CSharpDepsGraph.Cli.CommandLine;
using CSharpDepsGraph.Cli.Options;
using CSharpDepsGraph.Transforming.Filtering;
using NSubstitute;
using System.CommandLine.Parsing;

namespace CSharpDepsGraph.Cli.Tests.CommandLine;

public abstract class BaseExportCommandTests<TOptions> where TOptions : ExportOptions
{
    private readonly string _baseCommand;

    protected BaseExportCommandTests(string baseCommand)
    {
        _baseCommand = baseCommand;
    }

    [Test]
    public async Task BuildDefaults()
    {
        var defaultOptions = new GraphBuildOptions();
        await Check("dummy.sln", (b, e) =>
        {
            Assert.That(b.Properties, Is.Empty);
            Assert.That(b.GraphOptions, Is.Not.Null);
            Assert.That(b.GraphOptions.CreateLinksToSelf, Is.EqualTo(defaultOptions.CreateLinksToSelf));
            Assert.That(b.GraphOptions.CreateLinksToPrimitiveTypes, Is.EqualTo(defaultOptions.CreateLinksToPrimitiveTypes));
            Assert.That(b.GraphOptions.ParseGeneratedCode, Is.EqualTo(defaultOptions.ParseGeneratedCode));
            Assert.That(b.GraphOptions.SplitAssembliesVersions, Is.EqualTo(defaultOptions.SplitAssembliesVersions));
            Assert.That(b.GraphOptions.FullyQualifiedUid, Is.EqualTo(defaultOptions.FullyQualifiedUid));
            Assert.That(b.GraphOptions.AssemblyFilter, Is.EquivalentTo(defaultOptions.AssemblyFilter));
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
    public async Task OutputFileName()
    {
        var currentDir = Directory.GetCurrentDirectory();

        await Check("dummy.sln -o foo.txt", (b, e) =>
            Assert.That(e.OutputFileName, Is.EqualTo(Path.Combine(currentDir, "foo.txt")))
            );
        await Check("dummy.sln -o \"foo bar.txt\"", (b, e) =>
            Assert.That(e.OutputFileName, Is.EqualTo(Path.Combine(currentDir, "foo bar.txt")))
        );
        await Check("dummy.sln --output foo.txt", (b, e) =>
            Assert.That(e.OutputFileName, Is.EqualTo(Path.Combine(currentDir, "foo.txt")))
            );
        await Check("dummy.sln --output \"foo bar.txt\"", (b, e) =>
            Assert.That(e.OutputFileName, Is.EqualTo(Path.Combine(currentDir, "foo bar.txt")))
        );

        await Check("dummy.sln --output C:/foo/bar.txt", (b, e) =>
            Assert.That(e.OutputFileName, Is.EqualTo(Path.Combine(currentDir, Path.Combine("C:", "foo", "bar.txt"))))
        );
    }

    [Test]
    public async Task HideExternal()
    {
        await Check("dummy.sln --hide-external", (b, e) => Assert.That(e.HideExternal, Is.True));
    }

    [Test]
    public async Task NodeFilters()
    {
        await Check("dummy.sln --node-filter hide,foo*", (b, e) =>
        {
            Assert.That(e.NodeFilters.Single().FilterAction, Is.EqualTo(FilterAction.Hide));
            Assert.That(e.NodeFilters.Single().Pattern, Is.EqualTo("foo*"));
        });

        await Check("dummy.sln --node-filter hide,\"foo* bar\"", (b, e) =>
        {
            Assert.That(e.NodeFilters.Single().FilterAction, Is.EqualTo(FilterAction.Hide));
            Assert.That(e.NodeFilters.Single().Pattern, Is.EqualTo("foo* bar"));
        });

        await Check("dummy.sln --node-filter hide,foo* --node-filter skip,bar*", (b, e) =>
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

        await CheckError("dummy.sln --node-filter --hide-external", (e) =>
        {
            Assert.That(e, Does.Contain("node-filter"));
        });

        await CheckError("dummy.sln --node-filter bar,foo*", (e) =>
        {
            Assert.That(e, Does.Contain("bar"));
            Assert.That(e, Does.Contain("node-filter"));
        });

        await CheckError("dummy.sln --node-filter skip,", (e) =>
        {
            Assert.That(e, Does.Contain("skip"));
            Assert.That(e, Does.Contain("node-filter"));
        });

        await CheckError("dummy.sln --node-filter ,foo*", (e) =>
        {
            Assert.That(e, Does.Contain("foo*"));
            Assert.That(e, Does.Contain("node-filter"));
        });
    }

    [Test]
    public async Task ParseGeneratedCode()
    {
        await Check("dummy.sln --parse-generated", (b, e) => Assert.That(b.GraphOptions.ParseGeneratedCode, Is.True));
    }

    [Test]
    public async Task CreateLinksToSelf()
    {
        await Check("dummy.sln --links-to-self", (b, e) => Assert.That(b.GraphOptions.CreateLinksToSelf, Is.True));
    }

    [Test]
    public async Task CreateLinksToPrimitiveTypes()
    {
        await Check("dummy.sln --links-to-primitives", (b, e) => Assert.That(b.GraphOptions.CreateLinksToPrimitiveTypes, Is.True));
    }

    [Test]
    public async Task SplitAssembliesVersions()
    {
        await Check("dummy.sln --split-asm-versions", (b, e) => Assert.That(b.GraphOptions.SplitAssembliesVersions, Is.True));
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

    protected async Task<string?> Run(string commandArguments, ICommandFactory commandFactory)
    {
        CreateDummySln();

        var args = GetArgs(_baseCommand + " " + commandArguments);

        var originalStdOut = Console.Out;
        var originalStdErrOut = Console.Error;
        try
        {
            using var outWriter = new StringWriter();
            using var errWriter = new StringWriter();
            Console.SetOut(outWriter);
            Console.SetError(errWriter);

            var result = await Program.Run(args, commandFactory);
            if (result == 0)
            {
                return null;
            }

            return errWriter.ToString();
        }
        finally
        {
            Console.SetOut(originalStdOut);
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