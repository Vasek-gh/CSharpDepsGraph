namespace CSharpDepsGraph.Tests;

public static class TestData
{
    public const string TestProjectName = "TestProject";

    public const string Path = "TestData";
    public const string TestProjectPath = Path + $"/{TestProjectName}";
    public const string TestProjectSolution = Path + $"/{TestProjectName}.sln";

    public const string SkipBuildVar = "SKIP_BUILD";
}