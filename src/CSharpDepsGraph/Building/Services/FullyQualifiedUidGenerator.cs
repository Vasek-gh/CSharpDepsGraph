using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;

namespace CSharpDepsGraph.Building.Services;

internal class FullyQualifiedUidGenerator : ISymbolUidGenerator
{
    private readonly StringBuilder _stringBuilder;
    private readonly Dictionary<Version, string> _versionCache;
    private readonly GraphBuildOptions _options;

    private bool _parameterMode;
    private bool _excludeAssembly;

    public FullyQualifiedUidGenerator(GraphBuildOptions options)
    {
        _stringBuilder = new(200);
        _versionCache = new();
        _options = options;
    }

    public string Execute(ISymbol symbol)
    {
        _excludeAssembly = false;
        _stringBuilder.Clear();

        Append(symbol, true);

        var result = _stringBuilder.ToString();

        return result;
    }

    private void AppendAssembly(IAssemblySymbol symbol, bool last)
    {
        if (_excludeAssembly)
        {
            return;
        }

        _excludeAssembly = true;

        Append(symbol.Name);

        // todo It looks like there's a bug and the version isn't added when SplitAssembliesVersions is enabled.
        if (symbol.IsFromMetadata() && _options.SplitAssembliesVersions)
        {
            var version = symbol.Identity.Version;
            if (!_versionCache.TryGetValue(version, out var versionString))
            {
                versionString = version.ToString();
                _versionCache.Add(version, versionString);
            }

            Append("_");
            Append(versionString);
        }

        if (!last)
        {
            Append("/");
        }
    }

    private void AppendModule(IModuleSymbol symbol, bool last)
    {
        if (symbol.ContainingAssembly is not null)
        {
            AppendAssembly(symbol.ContainingAssembly, last);

            if (symbol.ContainingAssembly.Modules.Count() == 1)
            {
                return;
            }
        }

        Append(symbol.Name);
        if (!last)
        {
            Append("/");
        }
    }

    private void AppendNamespace(INamespaceSymbol symbol, bool last)
    {
        Append(symbol.ContainingSymbol, false);

        if (symbol.IsGlobalNamespace())
        {
            if (last)
            {
                Append("global::");
            }

            return;
        }

        Append(symbol.Name);
        if (!last)
        {
            Append(".");
        }
    }

    private void AppendType(ITypeSymbol symbol, bool last)
    {
        var nullableSuffix = "";

        if (symbol.NullableAnnotation == NullableAnnotation.Annotated)
        {
            nullableSuffix = "?";

            if (symbol.IsValueType
                && symbol is INamedTypeSymbol nullableTypeSymbol
                && nullableTypeSymbol.TypeArguments.Length == 1
            )
            {
                symbol = nullableTypeSymbol.TypeArguments[0];
            }
        }

        if (symbol is IPointerTypeSymbol pointerTypeSymbol)
        {
            AppendType(pointerTypeSymbol.PointedAtType, true);
            Append("*");
            Append(nullableSuffix);
            return;
        }

        if (symbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            AppendType(arrayTypeSymbol.ElementType, true);
            Append("[]");
            Append(nullableSuffix);
            return;
        }

        if (symbol is IDynamicTypeSymbol dynamicTypeSymbol)
        {
            Append(dynamicTypeSymbol.Name);
            Append(nullableSuffix);
            return;
        }

        var symbolName = GetTypeName(symbol);
        var symbolParent = GetTypeParent(symbol, _parameterMode);

        Append(symbolParent, false);
        Append(symbolName);

        if (symbol is INamedTypeSymbol namedTypeSymbol)
        {
            AppendTypeArguments(namedTypeSymbol.TypeArguments);
        }

        Append(nullableSuffix);
        if (!last)
        {
            Append(".");
        }
    }

    private void AppendEvent(IEventSymbol symbol)
    {
        Append(symbol.ContainingSymbol, false);

        symbol = HandleExplicitInterfaceImplementations(symbol, symbol.ExplicitInterfaceImplementations);

        var symbolName = symbol.Name;

        Append(symbolName);
    }

    private void AppendProperty(IPropertySymbol symbol)
    {
        Append(symbol.ContainingSymbol, false);

        symbol = HandleExplicitInterfaceImplementations(symbol, symbol.ExplicitInterfaceImplementations);

        var symbolName = symbol.Name;
        if (symbolName == "this[]")
        {
            symbolName = "this";
        }

        Append(symbolName);

        if (symbol.Parameters.Length > 0)
        {
            Append("[");
            AppendParameters(symbol.Parameters);
            Append("]");
        }
    }

    private void AppendMethod(IMethodSymbol symbol)
    {
        Append(symbol.ContainingSymbol, false);

        var symbolName = symbol.Name;

        if (symbol.MethodKind == MethodKind.Constructor)
        {
            symbolName = "ctor";
        }
        else if (symbol.MethodKind == MethodKind.StaticConstructor || symbol.MethodKind == MethodKind.SharedConstructor)
        {
            symbolName = "cctor";
        }
        else if (symbol.MethodKind == MethodKind.Destructor)
        {
            symbolName = "~";
        }
        else if (symbol.MethodKind == MethodKind.ExplicitInterfaceImplementation)
        {
            symbol = HandleExplicitInterfaceImplementations(symbol, symbol.ExplicitInterfaceImplementations);
            symbolName = symbol.Name;
        }

        Append(symbolName);
        AppendTypeArguments(symbol.TypeArguments);
        Append("(");
        AppendParameters(symbol.Parameters);
        Append(")");
    }

    private T HandleExplicitInterfaceImplementations<T>(T symbol, ImmutableArray<T> items)
        where T : ISymbol
    {
        if (items.Length == 0)
        {
            return symbol;
        }

        // C# doesn't support explicit implementation of two interfaces. However, there is
        // support at the IL level, and VB uses this
        if (items.Length > 1)
        {
            throw new NotImplementedException("Multiple explicit interface implementations");
        }

        var explicitSymbol = items[0];
        AppendType(explicitSymbol.ContainingType, false);
        return explicitSymbol;
    }

    private void AppendTypeArguments(ImmutableArray<ITypeSymbol> typeArguments, string delimiter = ",")
    {
        if (typeArguments.Length == 0)
        {
            return;
        }

        Append("<");

        for (var i = 0; i < typeArguments.Length; i++)
        {
            if (i > 0)
            {
                Append(delimiter);
            }

            AppendType(typeArguments[i], true);
        }

        Append(">");
    }

    private void AppendParameters(ImmutableArray<IParameterSymbol> parameters, string delimiter = ",")
    {
        if (parameters.Length == 0)
        {
            return;
        }

        _parameterMode = true;

        for (var i = 0; i < parameters.Length; i++)
        {
            if (i > 0)
            {
                Append(delimiter);
            }

            var parameter = parameters[i];

            Append(ToParameterPrefix(parameter.RefKind));
            AppendType(parameter.Type, true);
        }

        _parameterMode = false;

        string ToParameterPrefix(RefKind kind)
        {
            return kind switch
            {
                RefKind.Out => "out ",
                RefKind.Ref => "ref ",
                RefKind.In => "in ",
                RefKind.RefReadOnlyParameter => "ref readonly ",
                RefKind.None => string.Empty,
                _ => kind.ToString(),
            };

        }
    }

    private void Append(ISymbol? symbol, bool last)
    {
        if (symbol is null)
        {
            return;
        }

        if (symbol is IAssemblySymbol assemblySymbol)
        {
            AppendAssembly(assemblySymbol, last);
            return;
        }

        if (symbol is IModuleSymbol moduleSymbol)
        {
            AppendModule(moduleSymbol, last);
            return;
        }

        if (symbol is INamespaceSymbol namespaceSymbol)
        {
            AppendNamespace(namespaceSymbol, last);
            return;
        }

        if (symbol is ITypeSymbol typeSymbol)
        {
            AppendType(typeSymbol, last);
            return;
        }

        if (symbol is IEventSymbol eventSymbol)
        {
            AppendEvent(eventSymbol);
            return;
        }

        if (symbol is IPropertySymbol propertySymbol)
        {
            AppendProperty(propertySymbol);
            return;
        }

        if (symbol is IMethodSymbol methodSymbol)
        {
            AppendMethod(methodSymbol);
            return;
        }

        Append(symbol.ContainingSymbol, false);
        Append(symbol.Name);
    }

    private void Append(string? value)
    {
        _stringBuilder.Append(value);
    }

    private static string GetTypeName(ITypeSymbol typeSymbol)
    {
        return GetPrimitiveTypeName(typeSymbol) ?? typeSymbol.Name;
    }

    private static ISymbol? GetTypeParent(ITypeSymbol typeSymbol, bool parametersMode)
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

    private static string? GetPrimitiveTypeName(ITypeSymbol typeSymbol)
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
            _ => null
        };
    }
}