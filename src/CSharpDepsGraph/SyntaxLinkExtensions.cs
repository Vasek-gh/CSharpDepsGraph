using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph;

public static class SyntaxLinkExtensions
{
    public static string GetDisplayString(this INodeSyntaxLink syntaxLink)
    {
        var syntax = syntaxLink.Syntax;

        return syntax is null
            ? syntaxLink.Location
            : GetDisplayString(syntax);
    }

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

    public static string GetDisplayString(this ILinkSyntaxLink syntaxLink)
    {
        return GetDisplayString(syntaxLink.Syntax);
    }

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