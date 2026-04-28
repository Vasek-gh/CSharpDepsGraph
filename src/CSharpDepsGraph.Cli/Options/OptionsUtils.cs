using System.Collections;
using System.Runtime.CompilerServices;

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

    public static void AddOptionValue(
        this ICollection<KeyValuePair<string, string>> options,
        object? value,
        string valueCaption
        )
    {
        if (value is IEnumerable collection && value is not string)
        {
            var objectCollection = collection.Cast<object>();

            var collectionStr = objectCollection.Any()
                ? string.Join(", ", objectCollection.Select(v => v?.ToString() ?? "null"))
                : "[]";

            DoAddOptionValue(options, collectionStr, valueCaption);

            return;
        }

        var primitiveStr = value == null ? "null" : value.ToString();
        DoAddOptionValue(options, primitiveStr, valueCaption);
    }

    public static void AddOptionValue<T>(
        this ICollection<KeyValuePair<string, string>> options,
        IEnumerable<T> value,
        [CallerArgumentExpression(nameof(value))] string valueCaption = ""
        )
    {
        var str = value.Any()
            ? string.Join(", ", value.Select(v => v?.ToString() ?? "null"))
            : "[]";

        DoAddOptionValue(options, str, valueCaption);
    }

    public static void AddOptionValue<T>(
        this ICollection<KeyValuePair<string, string>> options,
        T? value,
        [CallerArgumentExpression(nameof(value))] string valueCaption = ""
        )
    {
        var str = value == null ? "null" : value.ToString();

        DoAddOptionValue(options, str, valueCaption);
    }

    public static void DoAddOptionValue(
        this ICollection<KeyValuePair<string, string>> options,
        string? value,
        string valueCaption
        )
    {
        var valueStr = value ?? "null";

        options.Add(new KeyValuePair<string, string>(RemoveInstanceName(valueCaption), valueStr));
    }

    private static string RemoveInstanceName(string caption)
    {
        var firstDotIndex = caption.IndexOf('.', StringComparison.InvariantCulture) + 1;

        return firstDotIndex < 0 || firstDotIndex == caption.Length
            ? caption
            : caption.Substring(firstDotIndex, caption.Length - firstDotIndex);
    }
}