namespace CSharpDepsGraph;

public static class NodeSyntaxLinkExtensions
{
    public static string GetDisplayString(this INodeSyntaxLink syntaxLink)
    {
        var syntax = syntaxLink.Syntax;
        if (syntax is null)
        {
            return syntaxLink.Location;
        }

        var lineSpan = syntax.SyntaxTree.GetLineSpan(syntax.Span);
        var line = lineSpan.StartLinePosition.Line + 1;
        var column = lineSpan.StartLinePosition.Character + 1;

        return $"{lineSpan.Path}:{line}:{column}";
    }
}