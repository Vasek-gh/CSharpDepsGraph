using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph;

/// <summary>
/// Extensions for syntax link classes
/// </summary>
public static class SyntaxLinkExtensions
{
    /// <summary>
    /// Convert <see cref="INodeSyntaxLink"/> to display string
    /// </summary>
    public static string GetDisplayString(this INodeSyntaxLink syntaxLink)
    {
        var syntax = syntaxLink.Syntax;

        return syntax is null
            ? syntaxLink.Location
            : GetDisplayString(syntax);
    }

    /// <summary>
    /// Convert <see cref="INodeSyntaxLink"/> to display string. <paramref name="pathConstructor"/> parameter is used to be able
    /// to replace the value of the file name
    /// </summary>
    public static string GetDisplayString(this INodeSyntaxLink syntaxLink, Func<string, string>? pathConstructor)
    {
        var syntax = syntaxLink.Syntax;
        if (syntaxLink.LocationKind == LocationKind.External)
        {
            return syntaxLink.Location;
        }

        return syntax is null
            ? GetDisplayString(syntaxLink.Location, pathConstructor)
            : GetDisplayString(syntax, pathConstructor);
    }

    /// <summary>
    /// Convert <see cref="ILinkSyntaxLink"/> to display string
    /// </summary>
    public static string GetDisplayString(this ILinkSyntaxLink syntaxLink)
    {
        return GetDisplayString(syntaxLink.Syntax);
    }

    /// <summary>
    /// Convert <see cref="ILinkSyntaxLink"/> to display string. <paramref name="pathConstructor"/> parameter is used to be able
    /// to replace the value of the file name
    /// </summary>
    public static string GetDisplayString(this ILinkSyntaxLink syntaxLink, Func<string, string>? pathConstructor)
    {
        return GetDisplayString(syntaxLink.Syntax, pathConstructor);
    }

    private static string GetDisplayString(SyntaxNode syntaxNode)
    {
        var path = syntaxNode.SyntaxTree.FilePath;
        path = path.Replace("\\", "/", StringComparison.Ordinal);

        var lineSpan = syntaxNode.SyntaxTree.GetLineSpan(syntaxNode.Span);
        var line = lineSpan.StartLinePosition.Line + 1;
        var column = lineSpan.StartLinePosition.Character + 1;

        return $"{path}:{line}:{column}";
    }

    private static string GetDisplayString(SyntaxNode syntaxNode, Func<string, string>? pathConstructor)
    {
        var path = GetDisplayString(syntaxNode.SyntaxTree.FilePath, pathConstructor);
        var lineSpan = syntaxNode.SyntaxTree.GetLineSpan(syntaxNode.Span);
        var line = lineSpan.StartLinePosition.Line + 1;
        var column = lineSpan.StartLinePosition.Character + 1;

        return $"{path}:{line}:{column}";
    }

    private static string GetDisplayString(string path, Func<string, string>? pathConstructor)
    {
        if (pathConstructor is not null)
        {
            path = pathConstructor(path);
        }

        return path.Replace("\\", "/", StringComparison.Ordinal);
    }
}