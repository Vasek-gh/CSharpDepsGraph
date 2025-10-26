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
    private readonly bool _withAssemblies;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultSymbolIdBuilder"/> class.
    /// </summary>
    public DefaultSymbolIdBuilder(bool withAssemblies)
    {
        _withAssemblies = withAssemblies;
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

        return symbol.Kind switch
        {
            SymbolKind.Assembly => GetAssemblyName(symbol),
            SymbolKind.NetModule => GetModuleName(symbol),
            SymbolKind.NamedType => GetTypeName(symbol),
            //SymbolKind.Method => GetMethodName(symbol),
            _ => WithAssembly(symbol, symbol.ToDisplayString())
        };
    }

    private static string GetAssemblyName(ISymbol symbol)
    {
        return $"{symbol.Name}.dll";
    }

    private static string GetModuleName(ISymbol symbol)
    {
        return $"{symbol.Name}.mdl";
    }

    private string GetTypeName(ISymbol symbol)
    {
        var baseId = symbol is ITypeSymbol typeSymbol && _predefinedTypes.Contains(typeSymbol.SpecialType)
            ? $"System.{symbol.ToDisplayString()}"
            : symbol.ToDisplayString();

        return WithAssembly(symbol, baseId);
    }

    private string WithAssembly(ISymbol symbol, string baseId)
    {
        if (symbol.IsGlobalNamespace())
        {
            baseId = $"global::";
        }

        if (!_withAssemblies)
        {
            return baseId;
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