using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using NUnit.Framework;
using System.Diagnostics;

namespace CSharpDepsGraph.Tests.Integrations;

[SetUpFixture]
public static class ProjectSource
{
    public static Solution Solution { get; private set; }
    public static MSBuildWorkspace Workspace { get; private set; }

    [OneTimeSetUp]
    public static async Task Init()
    {
        //await BuildSolution(TestData.TestProjectSolution);

        Workspace = MSBuildWorkspace.Create();
        Solution = await Workspace.OpenSolutionAsync(TestData.TestProjectSolution);
    }

    [OneTimeTearDown]
    public static void Done()
    {
        Workspace.Dispose();
    }

    private static async Task BuildSolution(string slnFileName)
    {
        if (Environment.GetEnvironmentVariable(TestData.SkipBuildVar) == "1")
        {
            await TestContext.Out.WriteLineAsync("Build test data skipped");
            return;
        }

        await TestContext.Out.WriteLineAsync($"Start build test data {slnFileName}...");

        using var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "dotnet",
                Arguments = $"build {slnFileName} --nologo -v n --disable-build-servers",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Environment = {
                    { "DOTNET_NOLOGO", "1" },
                    { "DOTNET_CLI_UI_LANGUAGE", "en" },
                    { "DOTNET_CLI_TELEMETRY_OPTOUT", "1" },
                    { "DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1" }
                }
            }
        };

        if (!process.Start())
        {
            throw new Exception("Fail to start build process");
        }

        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
        {
            var output = await process.StandardOutput.ReadToEndAsync();
            throw new Exception($"Build test data fail: {output}");
        }
    }
}