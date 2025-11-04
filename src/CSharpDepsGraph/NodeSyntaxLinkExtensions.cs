namespace CSharpDepsGraph;

public static class NodeSyntaxLinkExtensions
{
    public static string GetDisplayString(this INodeSyntaxLink syntaxLink)
    {
        var syntaxReference = syntaxLink.SyntaxReference;
        if (syntaxReference is null)
        {
            return syntaxLink.Location;
        }

        var lineSpan = syntaxReference.SyntaxTree.GetLineSpan(syntaxReference.Span);
        var line = lineSpan.StartLinePosition.Line + 1;
        var column = lineSpan.StartLinePosition.Character + 1;

        return $"{lineSpan.Path}:{line}:{column}";
    }
}