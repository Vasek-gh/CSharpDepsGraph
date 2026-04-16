namespace CSharpDepsGraph.Cli.Options;

internal static class OptionsUtils
{
    public static string? GetFileNameError(string fileName)
    {
        var extension = Path.GetExtension(fileName);

        if (extension != ".sln" && extension != ".slnx")
        {
            return $"Unsupported file type: {fileName}";
        }

        if (!File.Exists(fileName))
        {
            return $"File not found: {fileName}";
        }

        return null;
    }
}