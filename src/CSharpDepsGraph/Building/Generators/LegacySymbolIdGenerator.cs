using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpDepsGraph.Building.Generators;

internal class LegacySymbolIdGenerator : ISymbolIdGenerator
{
    private readonly ILogger<LegacySymbolIdGenerator> _logger;

    private int _callCount;
    private int _charsCount;
    private int _typeCount;

    public LegacySymbolIdGenerator(ILogger<LegacySymbolIdGenerator> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public void WriteStatistic()
    {
        _logger.LogDebug($"Call count: {_callCount}");
        _logger.LogDebug($"Chars count: {_charsCount}");
        _logger.LogDebug($"Types count: {_typeCount}");
    }

    /// <inheritdoc/>
    public string Execute(ISymbol symbol)
    {
        _callCount++;

        if (symbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            symbol = arrayTypeSymbol.ElementType;
        }

        var result = symbol.Kind switch
        {
            SymbolKind.Assembly => GetAssemblyName(symbol),
            SymbolKind.NetModule => GetModuleName(symbol),
            SymbolKind.NamedType => GetTypeName(symbol),
            _ => GetSymbolName(symbol)
        };

        _charsCount += result.Length;

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

    private string GetTypeName(ISymbol symbol)
    {
        _typeCount++;
        if (symbol is ITypeSymbol typeSymbol && GeneratorsUtils.IsTypePrimitive(typeSymbol))
        {
            return GetPredefinedTypeName(typeSymbol);
        }

        var baseId = symbol.ToDisplayString();
        return WithAssembly(symbol, baseId);
    }

    private static string GetPredefinedTypeName(ITypeSymbol symbol)
    {
        var baseId = symbol.ToDisplayString();
        return WithAssembly(symbol, baseId);
    }

    private static string GetSymbolName(ISymbol symbol)
    {
        if (symbol.IsTopLevelStatement())
        {
            return WithAssembly(symbol, "Main[TopLevel]");
        }

        return WithAssembly(symbol, symbol.ToDisplayString());
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