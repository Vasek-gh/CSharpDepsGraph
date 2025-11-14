using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CSharpDepsGraph.Building.Generators;

// todo append assembly always
public class SimpleSymbolIdGenerator : ISymbolIdGenerator
{
    private readonly ILogger _logger;
    private readonly bool _assemblyFullNames;
    private readonly StringBuilder _stringBuilder;
    private readonly Dictionary<string, string> _assemblyIdsMap;

    private int _idCounter;
    private int _callCount;
    private int _charsCount;
    private bool _excludeAssembly;
    private bool _parameterMode;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleSymbolIdGenerator"/> class.
    /// </summary>s
    public SimpleSymbolIdGenerator(ILogger<SimpleSymbolIdGenerator> logger, bool assemblyFullNames)
    {
        _logger = logger;
        _stringBuilder = new(200);
        _assemblyIdsMap = new(30);
        _assemblyFullNames = assemblyFullNames;
    }

    /// <inheritdoc/>
    public void WriteStatistic()
    {
        _logger.LogDebug($"Call count: {_callCount}");
        _logger.LogDebug($"Chars count: {_charsCount}");
    }

    /// <inheritdoc/>
    public string Execute(ISymbol symbol)
    {
        _callCount++;
        _excludeAssembly = false;
        _stringBuilder.Clear();

        Append(symbol, true);

        var result = _stringBuilder.ToString();
        _charsCount += result.Length;

        return result;
    }

    private void AppendAssembly(IAssemblySymbol symbol, bool last)
    {
        if (_excludeAssembly)
        {
            return;
        }

        _excludeAssembly = true;

        var name = symbol.Name;
        if (!_assemblyFullNames)
        {
            if (!_assemblyIdsMap.TryGetValue(symbol.Name, out var id))
            {
                id = _idCounter++.ToString(CultureInfo.InvariantCulture);
                _assemblyIdsMap.Add(symbol.Name, id);
            }

            name = id;
        }

        Append(name);
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

        var symbolName = GeneratorsUtils.GetTypeName(symbol);
        var symbolParent = GeneratorsUtils.GetTypeParent(symbol, _parameterMode);

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

        if (symbol.ExplicitInterfaceImplementations.Length > 0)
        {
            // todo C# не поддерживает явную реализацию двух интерфесов. Но есть поддержка на уровне IL и этим
            // пользуется vb. Надо подумать как тут лучше реализовать
            if (symbol.ExplicitInterfaceImplementations.Length > 1)
            {
                throw new NotImplementedException();
            }

            // todo что тут будет с генериком? Не возмет ли он генерики из декларации типа а не метода?
            var explicitSymbol = symbol.ExplicitInterfaceImplementations[0];
            AppendType(explicitSymbol.ContainingType, false);
            symbol = explicitSymbol;
        }

        var symbolName = symbol.Name;

        Append(symbolName);
    }

    private void AppendProperty(IPropertySymbol symbol)
    {
        Append(symbol.ContainingSymbol, false);

        if (symbol.ExplicitInterfaceImplementations.Length > 0)
        {
            // todo C# не поддерживает явную реализацию двух интерфесов. Но есть поддержка на уровне IL и этим
            // пользуется vb. Надо подумать как тут лучше реализовать
            if (symbol.ExplicitInterfaceImplementations.Length > 1)
            {
                throw new NotImplementedException();
            }

            // todo что тут будет с генериком? Не возмет ли он генерики из декларации типа а не метода?
            var explicitSymbol = symbol.ExplicitInterfaceImplementations[0];
            AppendType(explicitSymbol.ContainingType, false);
            symbol = explicitSymbol;
        }

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
            // todo C# не поддерживает явную реализацию двух интерфесов. Но есть поддержка на уровне IL и этим
            // пользуется vb. Надо подумать как тут лучше реализовать
            if (symbol.ExplicitInterfaceImplementations.Length > 1)
            {
                throw new NotImplementedException();
            }

            if (symbol.ExplicitInterfaceImplementations.Length == 1)
            {
                // todo что тут будет с генериком? Не возмет ли он генерики из декларации типа а не метода?
                var explicitSymbol = symbol.ExplicitInterfaceImplementations[0];
                AppendType(explicitSymbol.ContainingType, false);

                symbol = explicitSymbol;
                symbolName = explicitSymbol.Name;
            }
        }

        Append(symbolName);
        AppendTypeArguments(symbol.TypeArguments);
        Append("(");
        AppendParameters(symbol.Parameters);
        Append(")");
    }

    private void AppendExplicitInterfaceImplementation(ImmutableArray<IMethodSymbol> interfaces)
    {

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
            if (parameter.IsParams)
            {
                Append("params "); // todo kill???
            }

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
                _ => throw new Exception("todo"),
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
}