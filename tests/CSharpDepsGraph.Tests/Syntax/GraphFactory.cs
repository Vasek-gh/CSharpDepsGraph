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

namespace CSharpDepsGraph.Tests.Syntax;

[SetUpFixture]
public static class GraphFactory
{
    public static readonly string TestFileName = "AdbTestFile";

    private static Solution _baseSolution;
    private static AdhocWorkspace _workspace;
    private static MetadataReference[] _metadataReferences;
    private static ProjectInfo _testProject;

    [OneTimeSetUp]
    public static void Init()
    {
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

    public static IGraph CreateGraph(ILoggerFactory loggerFactory, string source)
    {
        var document = CreateDocument(TestFileName, source);
        var projectInfo = CreateProject(
            AsmName.Test,
            [document],
            [new ProjectReference(_testProject.Id)],
            _metadataReferences
            );

        var solution = _baseSolution.AddProject(projectInfo);

        return new GraphBuilder(loggerFactory)
            .Run(solution.Projects, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }

    private static ProjectInfo LoadTestProject(MetadataReference[] metadataReferences)
    {
        var files = new List<String>();

        files.AddRange(
            Directory.GetFiles($"{TestData.TestProjectPath}/Attributes").Select(p => p.Replace("\\", "/"))
        );
        files.AddRange(
            Directory.GetFiles($"{TestData.TestProjectPath}/Entities").Select(p => p.Replace("\\", "/"))
        );

        files.Add($"{TestData.TestProjectPath}/Statics.cs");
        files.Add($"{TestData.TestProjectPath}/Constants.cs");
        files.Add($"{TestData.TestProjectPath}/GenericClass1T.cs");
        files.Add($"{TestData.TestProjectPath}/GenericClass2T.cs");

        var documents = files.Select(f => CreateDocument(f))
            .Append(CreateDocument("globalUsings.cs", """
                global using global::System;
                global using global::System.Collections.Generic;
            """))
            .ToArray();

        return CreateProject(
            TestData.TestProjectName,
            documents,
            [],
            metadataReferences
            );
    }

    private static ProjectInfo CreateProject(
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