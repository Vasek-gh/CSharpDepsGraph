namespace CSharpDepsGraph;

public static class LinkSyntaxLinkExtensions
{
    public static string GetDisplayString(this ILinkSyntaxLink syntaxLink)
    {
        var syntax = syntaxLink.Syntax;

        var lineSpan = syntax.SyntaxTree.GetLineSpan(syntax.Span);
        var line = lineSpan.StartLinePosition.Line + 1;
        var column = lineSpan.StartLinePosition.Character + 1;

        return $"{lineSpan.Path}:{line}:{column}";
    }
}