using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace CSharpDepsGraph.Building.Generators;

internal static class GeneratorsUtils
{
    private static readonly HashSet<SpecialType> _primitiveTypes = new() {
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
    };

    public static bool IsTypePrimitive(ITypeSymbol typeSymbol)
    {
        return GetPrimitiveTypeName(typeSymbol) is not null;
    }

    public static string GetTypeName(ITypeSymbol typeSymbol)
    {
        return GetPrimitiveTypeName(typeSymbol) ?? typeSymbol.Name;
    }

    public static ISymbol? GetTypeParent(ITypeSymbol typeSymbol, bool parametersMode)
    {
        if (typeSymbol is ITypeParameterSymbol)
        {
            return null;
        }

        if (parametersMode && GetPrimitiveTypeName(typeSymbol) is not null)
        {
            return null;
        }

        return typeSymbol.ContainingSymbol;
    }

    public static string? GetPrimitiveTypeName(ITypeSymbol typeSymbol)
    {
        // todo надо это убить и тесты написать
        if (typeSymbol.ContainingAssembly.Name == "System.Runtime" && typeSymbol.ContainingAssembly.Identity.Version.Major >= 8)
        {
            return typeSymbol.SpecialType switch
            {
                SpecialType.System_Void => "void",
                SpecialType.System_Object => "object",
                SpecialType.System_Boolean => "bool",
                SpecialType.System_Char => "char",
                SpecialType.System_SByte => "sbyte",
                SpecialType.System_Byte => "byte",
                SpecialType.System_Int16 => "short",
                SpecialType.System_UInt16 => "ushort",
                SpecialType.System_Int32 => "int",
                SpecialType.System_UInt32 => "uint",
                SpecialType.System_Int64 => "long",
                SpecialType.System_UInt64 => "ulong",
                SpecialType.System_Decimal => "decimal",
                SpecialType.System_Single => "float",
                SpecialType.System_Double => "double",
                SpecialType.System_String => "string",
                SpecialType.System_IntPtr => "nint",
                SpecialType.System_UIntPtr => "nuint",
                //SpecialType.System_Enum => "enum",
                _ => null
            };
        }

        return typeSymbol.SpecialType switch
        {
            SpecialType.System_Void => "void",
            SpecialType.System_Object => "object",
            SpecialType.System_Boolean => "bool",
            SpecialType.System_Char => "char",
            SpecialType.System_SByte => "sbyte",
            SpecialType.System_Byte => "byte",
            SpecialType.System_Int16 => "short",
            SpecialType.System_UInt16 => "ushort",
            SpecialType.System_Int32 => "int",
            SpecialType.System_UInt32 => "uint",
            SpecialType.System_Int64 => "long",
            SpecialType.System_UInt64 => "ulong",
            SpecialType.System_Decimal => "decimal",
            SpecialType.System_Single => "float",
            SpecialType.System_Double => "double",
            SpecialType.System_String => "string",
            SpecialType.System_IntPtr => "IntPtr",
            SpecialType.System_UIntPtr => "UIntPtr",
            //SpecialType.System_Enum => "enum",
            _ => null
        };
    }
}