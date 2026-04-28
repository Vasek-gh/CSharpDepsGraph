namespace CSharpDepsGraph.Cli.Options;

public interface IOptions
{
    void Verbose(ICollection<KeyValuePair<string, string>> options);
}