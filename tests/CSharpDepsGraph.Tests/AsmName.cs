namespace CSharpDepsGraph.Tests;

internal class AsmName
{
    private const string _actualNetVersion = "10.0.0.0";

    public const string Test = "Test";
    public const string TestProject = "TestProject";
    public const string TestProjectCli = "TestProject.Cli";

    public const string CoreLib = $"System.Private.CoreLib_{_actualNetVersion}";
    public const string CoreLib80 = "System.Private.CoreLib_8.0.0.0";

    public const string Runtime = "System.Runtime";
    public const string Runtime60 = "System.Runtime_6.0.0.0";
    public const string Runtime80 = "System.Runtime_8.0.0.0";

    public const string Netstandard = "netstandard_2.1.0.0";

    public const string SystemConsole = $"System.Console_{_actualNetVersion}";
}