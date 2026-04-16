using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Integrations;

[SetUpFixture]
public static class ProjectSource
{
    public static Solution Solution { get; private set; }
    public static MSBuildWorkspace Workspace { get; private set; }

    [OneTimeSetUp]
    public static async Task Init()
    {
        Workspace = MSBuildWorkspace.Create();
        Solution = await Workspace.OpenSolutionAsync(TestData.TestProjectSolution);
    }

    [OneTimeTearDown]
    public static void Done()
    {
        Workspace.Dispose();
    }
}