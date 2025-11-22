using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace CSharpDepsGraph.Building;

internal class SymbolComparer
{
    private static readonly string _nameStub = "_";
    private static readonly Version _versionStub = new Version();

    private readonly bool _skipAssemblyVersion;
    private readonly bool _mergeSystemAssemblies;
    private readonly HashSet<string> _systemAssemblies;

    public SymbolComparer(
        bool skipAssemblyVersion,
        bool mergeSystemAssemblies,
        HashSet<string>? systemAssemblies
        )
    {
        _skipAssemblyVersion = skipAssemblyVersion;
        _mergeSystemAssemblies = mergeSystemAssemblies;
        _systemAssemblies = systemAssemblies ?? [
            "mscorlib",
            "netstandard",
            "System.Runtime"
        ];
    }

    public bool Compare(ISymbol? a, ISymbol? b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return Compare(a, b, false);
    }

    private bool Compare(ISymbol a, ISymbol b, bool parameterMode)
    {
        if (a.Kind != b.Kind)
        {
            return false;
        }

        var result = a.Kind switch
        {
            SymbolKind.Assembly => CompareAssembly(a as IAssemblySymbol, b as IAssemblySymbol, parameterMode),
            SymbolKind.NetModule => CompareModule(a as IModuleSymbol, b as IModuleSymbol, parameterMode),
            SymbolKind.Namespace => CompareNamespace(a as INamespaceSymbol, b as INamespaceSymbol, parameterMode),
            SymbolKind.Event => CompareEvent(a as IEventSymbol, b as IEventSymbol),
            SymbolKind.Property => CompareProperty(a as IPropertySymbol, b as IPropertySymbol),
            SymbolKind.Method => CompareMethod(a as IMethodSymbol, b as IMethodSymbol),
            _ when a is ITypeSymbol => CompareType(a as ITypeSymbol, b as ITypeSymbol, parameterMode),
            _ => CompareSymbol(a, b, parameterMode)
        };

        return result;
    }

    private bool CompareAssembly(IAssemblySymbol? a, IAssemblySymbol? b, bool parameterMode)
    {
        if (a is null
            || b is null
            )
        {
            return false;
        }

        var aInfo = GetAssemblyInfo(a);
        var bInfo = GetAssemblyInfo(b);

        return aInfo.name == bInfo.name && aInfo.version == bInfo.version;

        (string name, Version version) GetAssemblyInfo(IAssemblySymbol assemblySymbol)
        {
            var result = assemblySymbol.Name;
            if (parameterMode)
            {
                return Utils.CoreLibs.Contains(result) || result.StartsWith("System", StringComparison.Ordinal)
                    ? (_nameStub, _versionStub)
                    : (result, _versionStub);
            }

            if (!_mergeSystemAssemblies)
            {
                return (result, GetAssemblyVersion(assemblySymbol));
            }

            return _systemAssemblies.Contains(result)
                ? (_nameStub, _versionStub)
                : (result, GetAssemblyVersion(assemblySymbol));
        }

        Version GetAssemblyVersion(IAssemblySymbol assemblySymbol)
        {
            return _skipAssemblyVersion
                ? _versionStub
                : assemblySymbol.Identity.Version;
        }
    }

    private bool CompareModule(IModuleSymbol? a, IModuleSymbol? b, bool parameterMode)
    {
        if (a is null
            || b is null
            )
        {
            return false;
        }

        if (a.ContainingAssembly.Modules.Count() == 1 && b.ContainingAssembly.Modules.Count() == 1)
        {
            return CompareAssembly(a.ContainingAssembly, b.ContainingAssembly, parameterMode);
        }

        if (a.Name != b.Name)
        {
            return false;
        }

        return parameterMode || CompareAssembly(a.ContainingAssembly, b.ContainingAssembly, parameterMode);
    }

    private bool CompareNamespace(INamespaceSymbol? a, INamespaceSymbol? b, bool parameterMode)
    {
        if (a is null
            || b is null
            )
        {
            return false;
        }

        if (a.Name != b.Name)
        {
            return false;
        }

        return parameterMode || Compare(a.ContainingSymbol, b.ContainingSymbol, parameterMode);
    }

    private bool CompareType(ITypeSymbol? a, ITypeSymbol? b, bool parameterMode)
    {
        if (a is null
            || b is null
            || a.Kind != b.Kind
            || a.NullableAnnotation != b.NullableAnnotation
            )
        {
            return false;
        }

        if (a.NullableAnnotation == NullableAnnotation.Annotated)
        {
            a = GetNullableType(a);
            b = GetNullableType(b);
        }

        if (a.Kind == SymbolKind.PointerType)
        {
            return CompareType(GetPointerType(a), GetPointerType(b), parameterMode);
        }

        if (a.Kind == SymbolKind.ArrayType)
        {
            return CompareType(GetArrayType(a), GetArrayType(b), parameterMode);
        }

        if (a.Kind == SymbolKind.TypeParameter)
        {
            return a.Name == b.Name;
        }

        if (a is not INamedTypeSymbol aNamedTypeSymbol || b is not INamedTypeSymbol bNamedTypeSymbol)
        {
            return false;
        }

        if (GetPrimitiveTypeName(a) is not null && GetPrimitiveTypeName(a) == GetPrimitiveTypeName(b))
        {
            return true; // todo
        }

        var isSame = aNamedTypeSymbol.Arity == bNamedTypeSymbol.Arity
            && aNamedTypeSymbol.Name == bNamedTypeSymbol.Name;

        if (isSame && parameterMode)
        {
            return Compare(a.ContainingSymbol, b.ContainingSymbol, parameterMode);
        }

        return true;

        ITypeSymbol GetPointerType(ITypeSymbol typeSymbol)
        {
            var pointerTypeSymbol = typeSymbol as IPointerTypeSymbol
                ?? throw new InvalidCastException($"{typeSymbol} is not pointer");

            return pointerTypeSymbol.PointedAtType;
        }

        ITypeSymbol GetArrayType(ITypeSymbol typeSymbol)
        {
            var pointerTypeSymbol = typeSymbol as IArrayTypeSymbol
                ?? throw new InvalidCastException($"{typeSymbol} is not pointer");

            return pointerTypeSymbol.ElementType;
        }

        ITypeSymbol GetNullableType(ITypeSymbol typeSymbol)
        {
            if (typeSymbol.IsValueType
                && typeSymbol is INamedTypeSymbol nullableTypeSymbol
                && nullableTypeSymbol.TypeArguments.Length == 1
            )
            {
                return nullableTypeSymbol.TypeArguments[0];
            }

            return typeSymbol;
        }
    }

    private bool CompareEvent(IEventSymbol? a, IEventSymbol? b)
    {
        if (a is null
            || b is null
            || a.ExplicitInterfaceImplementations.Length != b.ExplicitInterfaceImplementations.Length
            || a.Name != b.Name
        )
        {
            return false;
        }

        if (!CompareSymbolArray(a.ExplicitInterfaceImplementations, b.ExplicitInterfaceImplementations, true))
        {
            return false;
        }

        return true;
    }

    private bool CompareProperty(IPropertySymbol? a, IPropertySymbol? b)
    {
        if (a is null
            || b is null
            || a.ExplicitInterfaceImplementations.Length != b.ExplicitInterfaceImplementations.Length
            || a.Name != b.Name
        )
        {
            return false;
        }

        if (!CompareSymbolArray(a.ExplicitInterfaceImplementations, b.ExplicitInterfaceImplementations, true))
        {
            return false;
        }

        return true;
    }

    private bool CompareMethod(IMethodSymbol? a, IMethodSymbol? b)
    {
        if (a is null
            || b is null
            || a.Arity != b.Arity
            || a.Parameters.Length != b.Parameters.Length
            || a.ExplicitInterfaceImplementations.Length != b.ExplicitInterfaceImplementations.Length
            || a.Name != b.Name
        )
        {
            return false;
        }

        for (var i = 0; i < a.Parameters.Length; i++)
        {
            var aParam = a.Parameters[i];
            var bParam = b.Parameters[i];

            if (aParam.RefKind != bParam.RefKind)
            {
                return false;
            }

            if (!Compare(aParam.Type, bParam.Type, true))
            {
                return false;
            }
        }

        if (!CompareSymbolArray(a.ExplicitInterfaceImplementations, b.ExplicitInterfaceImplementations, true))
        {
            return false;
        }

        return true;
    }

    private bool CompareSymbolArray<T>(ImmutableArray<T> aItems, ImmutableArray<T> bItems, bool parameterMode)
        where T : ISymbol
    {
        for (var i = 0; i < aItems.Length; i++)
        {
            if (!Compare(aItems[i], bItems[i], parameterMode))
            {
                return false;
            }
        }

        return true;
    }

    private bool CompareSymbol(ISymbol a, ISymbol b, bool parameterMode)
    {
        if (a is null
            || b is null
            || a.Name != b.Name)
        {
            return false;
        }

        return parameterMode || Compare(a.ContainingSymbol, b.ContainingSymbol, parameterMode);
    }

    public static string? GetPrimitiveTypeName(ITypeSymbol typeSymbol)
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
            SpecialType.System_IntPtr => "IntPtr",
            SpecialType.System_UIntPtr => "UIntPtr",
            //SpecialType.System_Enum => "enum",
            _ => null
        };
    }
}