using CSharpDepsGraph.Building;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace CSharpDepsGraph.Tests;

[SetUpFixture]
public static class GraphFactory
{
    public static readonly string TestFileName = "AdbTestFile.cs";

    private static Solution _baseSolution;
    private static AdhocWorkspace _workspace;
    private static MetadataReference[] _metadataReferences;
    private static ProjectInfo _testProject;

    [OneTimeSetUp]
    public static void Init()
    {
        MSBuildLocator.RegisterDefaults();

        _workspace = new AdhocWorkspace();
        _metadataReferences = CreateMetadataReferences();
        _testProject = LoadTestProject(_metadataReferences);

        var solutionInfo = SolutionInfo.Create(
            SolutionId.CreateNewId(),
            VersionStamp.Default,
            "Virtual.sln",
            [_testProject]
            );

        _baseSolution = _workspace.AddSolution(solutionInfo);
    }

    [OneTimeTearDown]
    public static void Done()
    {
        _workspace.Dispose();
    }

    public static IGraph CreateGraph(ILoggerFactory loggerFactory, string source, GraphBuildOptions? options)
    {
        var document = CreateDocument(TestFileName, source);
        var projectInfo = CreateProject(
            AsmName.Test + ".csproj",
            AsmName.Test,
            [document],
            [new ProjectReference(_testProject.Id)],
            _metadataReferences
            );

        options ??= new GraphBuildOptions();
        var solution = _baseSolution.AddProject(projectInfo);

        return new GraphBuilder(loggerFactory, options)
            .Run(solution.Projects, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }

    private static ProjectInfo LoadTestProject(MetadataReference[] metadataReferences)
    {
        var testProjectPath = TestData.TestProjectPath;
        var testProjectFilePath = Path.Combine(testProjectPath, TestData.TestProjectName) + ".csproj";

        var files = new List<string>();

        files.AddRange(
            Directory.GetFiles(Path.Combine(testProjectPath, "Attributes"))
        );
        files.AddRange(
            Directory.GetFiles(Path.Combine(testProjectPath, "Entities"))
        );

        files.Add(Path.Combine(testProjectPath, "Statics.cs"));
        files.Add(Path.Combine(testProjectPath, "Constants.cs"));
        files.Add(Path.Combine(testProjectPath, "GenericClass1T.cs"));
        files.Add(Path.Combine(testProjectPath, "GenericClass2T.cs"));

        var documents = files.Select(f => CreateDocument(f))
            .Append(CreateDocument("globalUsings.cs", """
                global using global::System;
                global using global::System.Collections.Generic;
            """))
            .ToArray();

        return CreateProject(
            testProjectFilePath,
            TestData.TestProjectName,
            documents,
            [],
            metadataReferences
            );
    }

    private static ProjectInfo CreateProject(
        string? filePath,
        string assemblyName,
        DocumentInfo[] documents,
        ProjectReference[] projectReferences,
        MetadataReference[] metadataReferences
        )
    {
        return ProjectInfo.Create(
            id: ProjectId.CreateNewId(),
            version: VersionStamp.Default,
            name: assemblyName,
            filePath: filePath,
            assemblyName: assemblyName,
            language: LanguageNames.CSharp,
            documents: documents,
            projectReferences: projectReferences,
            metadataReferences: metadataReferences,
            compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
    }

    private static DocumentInfo CreateDocument(string filename)
    {
        return CreateDocument(filename, File.ReadAllText(filename));
    }

    private static DocumentInfo CreateDocument(string filename, string content)
    {
        var projectId = ProjectId.CreateNewId();

        return DocumentInfo.Create(
            id: DocumentId.CreateNewId(projectId),
            name: filename,
            loader: TextLoader.From(SourceText.From(content).Container, VersionStamp.Default)
        );
    }

    private static MetadataReference[] CreateMetadataReferences()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(asm => !asm.IsDynamic)
            .Select(asm => MetadataReference.CreateFromFile(asm.Location))
            .ToArray();
    }
}