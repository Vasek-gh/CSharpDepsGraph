using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Cli.Commands;

internal static class LoggerExtensions
{
    public static void LogValue(
        this ILogger logger,
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

            DoLogValue(logger, collectionStr, valueCaption);

            return;
        }

        var primitiveStr = value == null ? "null" : value.ToString();
        DoLogValue(logger, primitiveStr, valueCaption);
    }

    public static void LogValue<T>(
        this ILogger logger,
        IEnumerable<T> value,
        [CallerArgumentExpression(nameof(value))] string valueCaption = ""
        )
    {
        var str = value.Any()
            ? string.Join(", ", value.Select(v => v?.ToString() ?? "null"))
            : "[]";

        DoLogValue(logger, str, valueCaption);
    }

    public static void LogValue<T>(
        this ILogger logger,
        T? value,
        [CallerArgumentExpression(nameof(value))] string valueCaption = ""
        )
    {
        var str = value == null ? "null" : value.ToString();

        DoLogValue(logger, str, valueCaption);
    }

    private static void DoLogValue(
        this ILogger logger,
        string? value,
        string valueCaption
        )
    {
        var valueStr = value ?? "null";

        logger.LogDebug($"{RemoveInstanceName(valueCaption)}: {valueStr}");
    }

    private static string RemoveInstanceName(string caption)
    {
        var firstDotIndex = caption.IndexOf('.', StringComparison.InvariantCulture) + 1;

        return firstDotIndex < 0 || firstDotIndex == caption.Length
            ? caption
            : caption.Substring(firstDotIndex, caption.Length - firstDotIndex);
    }
}