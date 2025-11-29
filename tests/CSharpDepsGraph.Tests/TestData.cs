namespace CSharpDepsGraph.Tests;

public static class TestData
{
    public const string TestProjectName = "TestProject";

    public static readonly string Path = System.IO.Path.GetFullPath("TestData");
    public static readonly string TestProjectPath = System.IO.Path.Combine(Path, TestProjectName);
    public static readonly string TestProjectSolution = System.IO.Path.Combine(Path, TestProjectName + ".sln");

    public const string SkipBuildVar = "SKIP_BUILD";
}