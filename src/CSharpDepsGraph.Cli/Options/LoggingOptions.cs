namespace CSharpDepsGraph.Cli.Options;

internal class LoggingOptions : IOptions
{
    public Verbosity Verbosity { get; set; }

    public void Verbose(ICollection<KeyValuePair<string, string>> options)
    {
        options.AddOptionValue(Verbosity);
    }
}
