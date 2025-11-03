using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CSharpDepsGraph.Building;

/// <summary>
/// The default unique identifier creator
/// </summary>
public class DefaultSymbolIdBuilder : ISymbolIdBuilder
{
    private readonly Dictionary<ISymbol, string> _symbolsCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultSymbolIdBuilder"/> class.
    /// </summary>
    public DefaultSymbolIdBuilder()
    {
        _symbolsCache = new(40_000, SymbolEqualityComparer.Default);
    }

    /// <inheritdoc/>
    public string Execute(ISymbol symbol)
    {
        if (symbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            symbol = arrayTypeSymbol.ElementType;
        }

        if (symbol.IsTopLevelStatement())
        {
            return WithAssembly(symbol, "Main[TopLevel]");
        }

        if (_symbolsCache.TryGetValue(symbol, out var cachedId))
        {
            return cachedId;
        }

        var result = symbol.Kind switch
        {
            SymbolKind.Assembly => GetAssemblyName(symbol),
            SymbolKind.NetModule => GetModuleName(symbol),
            SymbolKind.NamedType => GetTypeName(symbol),
            //SymbolKind.Method => GetMethodName(symbol),
            _ => WithAssembly(symbol, symbol.ToDisplayString())
        };

        _symbolsCache.Add(symbol, result);

        return result;
    }

    private static string GetAssemblyName(ISymbol symbol)
    {
        return $"{symbol.Name}.dll";
    }

    private static string GetModuleName(ISymbol symbol)
    {
        return $"{symbol.Name}.mdl";
    }

    private static string GetTypeName(ISymbol symbol)
    {
        if (symbol is ITypeSymbol typeSymbol && _predefinedTypes.Contains(typeSymbol.SpecialType))
        {
            return GetPredefinedTypeName(typeSymbol);
        }

        var baseId = symbol.ToDisplayString();
        return WithAssembly(symbol, baseId);
    }

    private static string GetPredefinedTypeName(ITypeSymbol symbol)
    {
        var baseId = $"System.{symbol.ToDisplayString()}";
        return WithAssembly(symbol, baseId);
    }

    private static string WithAssembly(ISymbol symbol, string baseId)
    {
        if (symbol.IsGlobalNamespace())
        {
            baseId = $"global::";
        }

        if (symbol.ContainingAssembly == null)
        {
            throw new Exception($"Symbol {baseId} has not assembly");
        }

        return symbol.ContainingAssembly.Modules.Count() == 1
            ? $"{GetAssemblyName(symbol.ContainingAssembly)}/{baseId}"
            : $"{GetAssemblyName(symbol.ContainingAssembly)}/{GetModuleName(symbol.ContainingModule)}/{baseId}";
    }

    private static readonly HashSet<SpecialType> _predefinedTypes = new() {
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
        SpecialType.System_DateTime,
    };
}