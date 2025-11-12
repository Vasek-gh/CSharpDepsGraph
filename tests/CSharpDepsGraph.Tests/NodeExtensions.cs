using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace CSharpDepsGraph.Tests;

internal static class NodeExtensions
{
    public static INode GetNode(this INode node, string assemblyName, string fullQualifiedName)
    {
        return node.GetNode(MakePath(assemblyName, fullQualifiedName));
    }

    public static INode GetNode(this INode node, string path)
    {
        return node.GetNodeInternal(path)
            ?? node.GetNodeInternal($"External.{path}")
            ?? throw new AssertionException($"Symbol {path} not found");
    }

    public static string MakePath(string assemblyName, string? fullQualifiedName)
    {
        return fullQualifiedName == null
            ? $"[{assemblyName}]"
            : $"[{assemblyName}].{fullQualifiedName}";
    }

    public static string MakePath(string assemblyName, params string?[] paths)
    {
        return paths.Length == 0
            ? $"[{assemblyName}]"
            : $"[{assemblyName}].{string.Join(".", paths.Where(p => !string.IsNullOrEmpty(p)))}";
    }

    private static INode? GetNodeInternal(this INode node, string path)
    {
        var subPaths = PathParser.Run(path);
        var lastPath = subPaths.Last();

        var result = node;
        foreach (var subPath in subPaths.Take(subPaths.Length - 1))
        {
            var child = FindChild(result, n => n.Symbol?.Name == subPath)
                ?? FindChild(result, n => n.Symbol?.ToDisplayString().Split('.').Last() == subPath)
                ?? FindChild(result, n => n.Id == subPath);

            if (child == null)
            {
                return null;
            }

            result = child;
        }

        if (result.Symbol == null)
        {
            // for assembly search
            var child = FindChild(result, n => n.Symbol?.Name == lastPath);
            if (child != null)
            {
                return child;
            }

            return child;
        }

        var resultId = result.Symbol.Kind == SymbolKind.Assembly || result.Symbol.Kind == SymbolKind.NetModule
            ? $"{result.Id}/{lastPath}"
            : $"{result.Id}.{lastPath}";

        return FindChild(result, n => n.Id == resultId);
    }

    private static INode? FindChild(INode node, Func<INode, bool> predicate)
    {
        return node.Childs.SingleOrDefault(predicate);
    }

    private class PathParser
    {
        private readonly List<string> _result;

        private readonly StringBuilder _stringBuilder;

        private char _groupSymbol;
        private int _groupSymbolCount;

        public PathParser()
        {
            _result = new List<string>();
            _stringBuilder = new StringBuilder();
        }

        public static string[] Run(string path)
        {
            return new PathParser().DoRun(path);
        }

        private string[] DoRun(string path)
        {
            foreach (var c in path)
            {
                if (c == '[' || c == '(' || c == '<')
                {
                    if (!IsInGroup() || c == _groupSymbol)
                    {
                        _groupSymbol = GetGroupCloseChar(c);
                        _groupSymbolCount++;
                    }

                    if (c == '[')
                    {
                        continue;
                    }
                }

                if (c == ']' || c == ')' || c == '>')
                {
                    if (_groupSymbol == char.MinValue)
                    {
                        throw new Exception("Group not open");
                    }

                    if (c == _groupSymbol)
                    {
                        _groupSymbolCount--;
                    }

                    if (_groupSymbolCount == 0)
                    {
                        _groupSymbol = Char.MinValue;
                    }

                    if (c == ']')
                    {
                        continue;
                    }
                }

                if (!IsInGroup() && c == '.')
                {
                    AppendBuilderToResult();
                    continue;
                }

                _stringBuilder.Append(c);
            }

            AppendBuilderToResult();

            return _result.ToArray();
        }

        private bool IsInGroup()
        {
            return _groupSymbolCount > 0;
        }

        private static char GetGroupCloseChar(char c)
        {
            return c switch
            {
                '(' => ')',
                '[' => ']',
                '<' => '>',
                _ => Char.MinValue
            };
        }

        private void AppendBuilderToResult()
        {
            if (IsInGroup())
            {
                throw new Exception("Group not close");
            }

            if (_stringBuilder.Length > 0)
            {
                _result.Add(_stringBuilder.ToString());
                _stringBuilder.Clear();
            }
        }
    }
}