namespace CSharpDepsGraph.Cli.Options;

internal class BuildOptions
{
    public string FileName { get; set; } = ""; // todo optional / FileName -> FilePath

    public string? Configuration { get; set; } // todo kill

    public IEnumerable<KeyValuePair<string, string>> Properties { get; set; } = [];

    public BuildOptions Validate()
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

        return new BuildOptions()
        {
            FileName = FileName,
            Configuration = string.IsNullOrWhiteSpace(Configuration) ? null : Configuration,
            Properties = Properties
        };
    }
}