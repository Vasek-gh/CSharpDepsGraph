using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace CSharpDepsGraph.Building;

internal class Utils
{
    public static ILogger CreateLogger<T>(ILoggerFactory factory, string entityName)
    {
        return factory.CreateLogger($"{typeof(T).Name}<{entityName}>");
    }

    public static IEnumerable<SyntaxLink> CreateExternalSyntaxLink(ISymbol symbol)
    {
        return CreateAssemblySyntaxLink(symbol, SyntaxFileKind.External);
    }

    public static IEnumerable<SyntaxLink> CreateAssemblySyntaxLink(ISymbol symbol, SyntaxFileKind kind)
    {
        return new[]
        {
            new SyntaxLink()
            {
                FileKind = kind,
                Path = symbol is IAssemblySymbol
                    ? symbol.Name
                    : symbol.ContainingAssembly.Name
            }
        };
    }

    public static SyntaxLink CreateSyntaxLink(
        SyntaxNode syntax,
        SyntaxFileKind syntaxFileKind,
        FileLinePositionSpan lineSpan
        )
    {
        return new SyntaxLink()
        {
            FileKind = syntaxFileKind,
            Path = lineSpan.Path,
            Syntax = syntax,
            Line = lineSpan.StartLinePosition.Line + 1,
            Column = lineSpan.StartLinePosition.Character + 1
        };
    }

    public static string GetSyntaxLocation(SyntaxNode syntax)
    {
        var span = syntax.SyntaxTree.GetLineSpan(syntax.Span);
        var line = span.StartLinePosition.Line + 1;
        var column = span.StartLinePosition.Character + 1;
        var path = span.Path;

        return $"{path}:{line}:{column}";
    }
}