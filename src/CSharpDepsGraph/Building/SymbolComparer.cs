using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace CSharpDepsGraph.Building;

internal class SymbolComparer
{
    private static readonly string _nameStub = "_";
    private static readonly Version _versionStub = new Version();

    private readonly GraphBuildOptions _options;

    public SymbolComparer(
        GraphBuildOptions options
        )
    {
        _options = options;
    }

    public bool Compare(ISymbol? a, ISymbol? b, bool withParents)
    {
        return Compare(a, b, withParents, false);
    }

    private bool Compare(ISymbol? a, ISymbol? b, bool withParents, bool parameterMode)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

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

        if (result && withParents)
        {
            a = a.ContainingSymbol;
            b = b.ContainingSymbol;
            result = Compare(a, b, true, false);
        }

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

            return (result, GetAssemblyVersion(assemblySymbol));
        }

        Version GetAssemblyVersion(IAssemblySymbol assemblySymbol)
        {
            return _options.DoNotMergeAssembliesWithDifferentVersions
                ? assemblySymbol.Identity.Version
                : _versionStub;
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

        return parameterMode || Compare(a.ContainingSymbol, b.ContainingSymbol, false, parameterMode);
    }

    private bool CompareType(ITypeSymbol? a, ITypeSymbol? b, bool parameterMode)
    {
        if (a is null
            || b is null
            || a.Kind != b.Kind
            || IsNullable(a) != IsNullable(b)
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

        if (ComparePrimiteTypes(a, b))
        {
            return true;
        }

        if (aNamedTypeSymbol.Name != bNamedTypeSymbol.Name
            || !CompareTypeParameters(aNamedTypeSymbol, bNamedTypeSymbol)
            )
        {
            return false;
        }

        return parameterMode || Compare(a.ContainingSymbol, b.ContainingSymbol, false, parameterMode);

        ITypeSymbol GetPointerType(ITypeSymbol typeSymbol)
        {
            var pointerTypeSymbol = Utils.CheckNull(typeSymbol as IPointerTypeSymbol, $"{typeSymbol} is not pointer");

            return pointerTypeSymbol.PointedAtType;
        }

        ITypeSymbol GetArrayType(ITypeSymbol typeSymbol)
        {
            var pointerTypeSymbol = Utils.CheckNull(typeSymbol as IArrayTypeSymbol, $"{typeSymbol} is not array");

            return pointerTypeSymbol.ElementType;
        }

        bool IsNullable(ITypeSymbol typeSymbol)
        {
            return typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
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

        bool CompareTypeParameters(INamedTypeSymbol aTypeSymbol, INamedTypeSymbol bTypeSymbol)
        {
            if (!parameterMode)
            {
                return aTypeSymbol.Arity == bTypeSymbol.Arity;
            }

            if (aNamedTypeSymbol.TypeArguments.Length != bTypeSymbol.TypeArguments.Length)
            {
                return false;
            }

            for (var i = 0; i < aTypeSymbol.TypeArguments.Length; i++)
            {
                if (!CompareType(aTypeSymbol.TypeArguments[i], bTypeSymbol.TypeArguments[i], parameterMode))
                {
                    return false;
                }
            }

            return true;
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
            || a.IsIndexer != b.IsIndexer
            || a.Parameters.Length != b.Parameters.Length
            || a.ExplicitInterfaceImplementations.Length != b.ExplicitInterfaceImplementations.Length
            || a.Name != b.Name
            || !CompareParameters(a.Parameters, b.Parameters)
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
            || !CompareParameters(a.Parameters, b.Parameters)
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

    private bool CompareParameters(ImmutableArray<IParameterSymbol> aItems, ImmutableArray<IParameterSymbol> bItems)
    {
        if (aItems.Length != bItems.Length)
        {
            return false;
        }

        for (var i = 0; i < aItems.Length; i++)
        {
            var aParam = aItems[i];
            var bParam = bItems[i];

            if (aParam.RefKind != bParam.RefKind)
            {
                return false;
            }

            if (!CompareType(aParam.Type, bParam.Type, true))
            {
                return false;
            }
        }

        return true;
    }

    private bool CompareSymbolArray<T>(ImmutableArray<T> aItems, ImmutableArray<T> bItems, bool parameterMode)
        where T : ISymbol
    {
        for (var i = 0; i < aItems.Length; i++)
        {
            if (!Compare(aItems[i], bItems[i], false, parameterMode))
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

        return parameterMode || Compare(a.ContainingSymbol, b.ContainingSymbol, false, parameterMode);
    }

    public static bool ComparePrimiteTypes(ITypeSymbol aTypeSymbol, ITypeSymbol bTypeSymbol)
    {
        var aIsPrimitive = Utils.IsPrimiteType(aTypeSymbol);
        var bIsPrimitive = Utils.IsPrimiteType(bTypeSymbol);

        if (aIsPrimitive != bIsPrimitive)
        {
            return false;
        }

        if (!aIsPrimitive && !bIsPrimitive)
        {
            return false;
        }

        return aTypeSymbol.SpecialType == bTypeSymbol.SpecialType;
    }
}