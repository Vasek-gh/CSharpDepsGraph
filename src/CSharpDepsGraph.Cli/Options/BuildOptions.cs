using CSharpDepsGraph.Building;

namespace CSharpDepsGraph.Cli.Options;

public class BuildingOptions : IOptions
{
    public string FileName { get; set; } = "";

    public IEnumerable<KeyValuePair<string, string>> Properties { get; set; } = [];

    public GraphBuildOptions GraphOptions { get; set; } = new GraphBuildOptions();

    public BuildingOptions Validate()
    {
        var fileNameError = OptionsUtils.GetFileNameError(FileName);
        if (fileNameError != null)
        {
            throw new ArgumentException(fileNameError);
        }

        foreach (var prop in Properties)
        {
            if (string.IsNullOrWhiteSpace(prop.Key))
            {
                throw new ArgumentException("Property must have name");
            }

            if (string.IsNullOrWhiteSpace(prop.Value))
            {
                throw new ArgumentException("Property must have value");
            }
        }

        return new BuildingOptions()
        {
            FileName = FileName,
            Properties = Properties,
            GraphOptions = GraphOptions
        };
    }

    public void Verbose(ICollection<KeyValuePair<string, string>> options)
    {
        options.AddOptionValue(FileName);
        options.AddOptionValue(Properties);
        options.AddOptionValue(GraphOptions.CreateLinksToSelf);
        options.AddOptionValue(GraphOptions.CreateLinksToPrimitiveTypes);
        options.AddOptionValue(GraphOptions.ParseGeneratedCode);
        options.AddOptionValue(GraphOptions.SplitAssembliesVersions);
        options.AddOptionValue(GraphOptions.FullyQualifiedUid);
        options.AddOptionValue(GraphOptions.AssemblyFilter);
    }
}