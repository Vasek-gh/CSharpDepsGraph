using CSharpDepsGraph.Building.Entities;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace CSharpDepsGraph.Building;

internal static class Utils
{
    private static readonly Dictionary<string, INodeSyntaxLink> _assemblyLinksCache = new();

    public static readonly HashSet<SpecialType> PrimitiveTypes = [
        SpecialType.System_Void,
        SpecialType.System_Object,
        SpecialType.System_Boolean,
        SpecialType.System_Char,
        SpecialType.System_SByte,
        SpecialType.System_Byte,
        SpecialType.System_Int16,
        SpecialType.System_UInt16,
        SpecialType.System_Int32,
        SpecialType.System_UInt32,
        SpecialType.System_Int64,
        SpecialType.System_UInt64,
        SpecialType.System_Decimal,
        SpecialType.System_Single,
        SpecialType.System_Double,
        SpecialType.System_String,
        SpecialType.System_IntPtr,
        SpecialType.System_UIntPtr,
        SpecialType.System_DateTime
    ];

    public static readonly HashSet<string> CoreLibs = [
        "mscorlib",
        "netstandard",
        "System.Runtime"
    ];

    public static ILogger CreateLogger<T>(ILoggerFactory factory, string? entityName)
    {
        var category = entityName is null
            ? typeof(T).Name
            : $"{typeof(T).Name}.{entityName}";

        return factory.CreateLogger(category);
    }

    public static INodeSyntaxLink CreateAssemblySyntaxLink(string path)
    {
        if (!_assemblyLinksCache.TryGetValue(path, out var link))
        {
            link = new AssemblyNodeSyntaxLink(path);
            _assemblyLinksCache.Add(path, link);
        }

        return link;
    }

    public static bool IsPrimitiveType(ISymbol symbol)
    {
        return symbol is ITypeSymbol typeSymbol && IsPrimitiveType(typeSymbol);
    }

    public static bool IsPrimitiveType(ITypeSymbol typeSymbol)
    {
        return PrimitiveTypes.Contains(typeSymbol.SpecialType);
    }

    public static T CheckNull<T>([NotNull] T? value, string errorMessage)
    {
        if (value is null)
        {
            throw new Exception(errorMessage);
        }

        return value;
    }

    public static List<T> GetEmptyList<T>()
    {
        return GenericHost<T>.EmptyList;
    }

    private class GenericHost<T>
    {
        public static List<T> EmptyList = [];
    }
}