namespace CSharpDepsGraph.Cli.Commands.Settings;

internal class LoggingSettings
{
    public Verbosity Verbosity { get; set; }

    public static class Defaults
    {
        public static Verbosity Verbosity { get; } = Verbosity.Normal;
    }
}
