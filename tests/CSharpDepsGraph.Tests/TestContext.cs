using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace CSharpDepsGraph.Tests;

[SetUpFixture]
public class TestContext
{
    public static readonly string TestFileName = "AdbTestFile";

    public static TestContext Instance { get; }

    public Solution BaseSolution { get; }
    public IEnumerable<MetadataReference> MetadataRefs { get; }

    static TestContext()
    {
        Instance = new TestContext(true);
    }

    public TestContext()
    {
        BaseSolution = null!;
        MetadataRefs = null!;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter")]
    public TestContext(bool staticInitTag)
    {
        BaseSolution = GetBaseSolution();
        MetadataRefs = GetMetadataRefs();
    }

    public async Task<Solution> CreateSolutionAsync(
        string? additionalTestSourceCode,
        CancellationToken cancellationToken = default
        )
    {
        var solution = BaseSolution;

        if (additionalTestSourceCode != null)
        {
            var projectInfo = GetAdditionalProject(additionalTestSourceCode);

            solution = BaseSolution.AddProject(projectInfo);

            var project = solution.GetProject(projectInfo.Id)
                ?? throw new InvalidOperationException(nameof(Solution.GetProject));

            var compilation = await project.GetCompilationAsync(CancellationToken.None)
                ?? throw new InvalidOperationException(nameof(Project.GetCompilationAsync));

            var diagErrors = compilation.GetDiagnostics(cancellationToken)
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.GetMessage(CultureInfo.InvariantCulture));

            if (diagErrors.Any())
            {
                throw new AggregateException("Invalid syntax", diagErrors.Select(e => new Exception(e)));
            }
        }

        return solution;
    }

    private ProjectInfo GetAdditionalProject(string content)
    {
        var projectId = ProjectId.CreateNewId();

        var document = DocumentInfo.Create(
            id: DocumentId.CreateNewId(projectId),
            name: TestFileName,
            loader: TextLoader.From(SourceText.From(content).Container, VersionStamp.Default)
        );

        return ProjectInfo.Create(
            id: ProjectId.CreateNewId(),
            version: VersionStamp.Default,
            name: AsmName.Test,
            assemblyName: AsmName.Test,
            language: LanguageNames.CSharp,
            documents: new[] { document },
            metadataReferences: MetadataRefs,
            compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
            projectReferences: BaseSolution.Projects.Select(p => new ProjectReference(p.Id))
        );
    }

    private static Solution GetBaseSolution()
    {
        var slnFileName = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "./",
            "TestProject",
            "TestProject.sln"
        );

        if (!File.Exists(slnFileName))
        {
            throw new Exception($"Solution file {slnFileName} not found");
        }

        BuildSolution(slnFileName);

        // If this is called before the "dotnet build" is called, the build will fail
        MSBuildLocator.RegisterDefaults();

#pragma warning disable CA2000
        return MSBuildWorkspace.Create() // todo check CA2000
            .OpenSolutionAsync(slnFileName)
            .GetAwaiter()
            .GetResult();
#pragma warning restore CA2000
    }

    private static void BuildSolution(string slnFileName)
    {
        using var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "dotnet",
                Arguments = $"build {slnFileName} --nologo -v q --disable-build-servers",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                Environment = {
                    { "DOTNET_CLI_UI_LANGUAGE", "en" }
                }
            }
        };

        if (!process.Start())
        {
            throw new Exception("Fail to start build process");
        }

        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            var output = process.StandardOutput.ReadToEnd();
            throw new Exception($"Build TestProject fail: {output}");
        }
    }

    private static IEnumerable<MetadataReference> GetMetadataRefs()
    {
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureDefaults(null);
        builder.Build();

        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(asm => !asm.IsDynamic)
            .Select(asm => MetadataReference.CreateFromFile(asm.Location));
    }
}