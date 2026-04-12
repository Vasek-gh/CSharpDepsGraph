using NUnit.Framework;

namespace CSharpDepsGraph.Tests;

[SetUpFixture]
public static class Initialization
{
    [OneTimeSetUp]
    public static void Init()
    {
        //Environment.SetEnvironmentVariable(TestData.SkipBuildVar, "1");
    }

    [OneTimeTearDown]
    public static void Done()
    {
    }
}