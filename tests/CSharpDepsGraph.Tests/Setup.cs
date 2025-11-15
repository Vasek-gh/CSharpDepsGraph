using Microsoft.Build.Locator;
using NUnit.Framework;
using System;

namespace CSharpDepsGraph.Tests;

[SetUpFixture]
public static class SetUp
{
    [OneTimeSetUp]
    public static void Init()
    {
        Environment.SetEnvironmentVariable(TestData.SkipBuildVar, "1");

        MSBuildLocator.RegisterDefaults();

        GraphFactory.Init();
    }

    [OneTimeTearDown]
    public static void Done()
    {
        GraphFactory.Done();
    }
}