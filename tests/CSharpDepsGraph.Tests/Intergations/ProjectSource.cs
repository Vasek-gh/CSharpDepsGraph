using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CSharpDepsGraph.Tests.Intergations;

[SetUpFixture]
public static class ProjectSource
{
    public static Solution Solution { get; private set; }
    public static MSBuildWorkspace Workspace { get; private set; }

    [OneTimeSetUp]
    public static async Task Init()
    {
        BuildSolution(TestData.TestProjectSolution);

        Workspace = MSBuildWorkspace.Create();
        Solution = await Workspace.OpenSolutionAsync(TestData.TestProjectSolution);
    }

    [OneTimeTearDown]
    public static void Done()
    {
        Workspace.Dispose();
    }

    private static void BuildSolution(string slnFileName)
    {
        if (Environment.GetEnvironmentVariable(TestData.SkipBuildVar) == "1")
        {
            return;
        }

        using var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "dotnet",
                Arguments = $"build {slnFileName} --nologo -v q --disable-build-servers",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                Environment = {
                    { "DOTNET_CLI_UI_LANGUAGE", "en" } // todo disable telemetry
                }
            }
        };

        if (!process.Start())
        {
            throw new Exception("Fail to start restore process");
        }

        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            var output = process.StandardOutput.ReadToEnd();
            throw new Exception($"Restore {slnFileName} fail: {output}");
        }
    }
}