using CSharpDepsGraph.Building;
using Microsoft.CodeAnalysis;
using Moq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpDepsGraph.Tests.Transformation;

internal static class Mocks
{
    public static readonly ISymbolIdBuilder SymbolIdBuilder = new DefaultSymbolIdBuilder();

    public static IGraph CreateGraph(IEnumerable<INode> nodes, IEnumerable<ILink> links)
    {
        var rootMock = new Mock<INode>();
        rootMock.Setup(m => m.Id).Returns(GraphConsts.RootNodeId);
        rootMock.Setup(m => m.Childs).Returns(nodes);
        rootMock.Setup(m => m.SyntaxLinks).Returns([]);

        var graphMock = new Mock<IGraph>();
        graphMock.Setup(m => m.Root).Returns(rootMock.Object);
        graphMock.Setup(m => m.Links).Returns(links);

        return graphMock.Object;
    }

    public static ILink CreateLink(INode source, INode target)
    {
        var mock = Substitute.For<ILink>();
        mock.Source.Returns(source);
        mock.OriginalSource.Returns(source);
        mock.Target.Returns(target);
        mock.OriginalTarget.Returns(target);

        mock.SyntaxLink.Returns(new SyntaxLink()
        {
            FileKind = LocationKind.Local,
            Path = ""
        });

        return mock;
    }

    public static IAssemblySymbol CreateAssemblySymbol(string name)
    {
        var moduleSymbolMock = Substitute.For<IModuleSymbol>();
        moduleSymbolMock.Name.Returns("Module1");
        moduleSymbolMock.Kind.Returns(SymbolKind.NetModule);

        var assemblySymbolMock = Substitute.For<IAssemblySymbol>();
        assemblySymbolMock.Name.Returns(name);
        assemblySymbolMock.Kind.Returns(SymbolKind.Assembly);
        assemblySymbolMock.Modules.Returns<IEnumerable<IModuleSymbol>>(new[] { moduleSymbolMock });
        assemblySymbolMock.ToDisplayString().Returns(name);

        return assemblySymbolMock;
    }

    public static NodeMock CreateAssemblyNode(string name, NodeMock? parent)
    {
        var assemblySymbol = CreateAssemblySymbol(name);

        var node = new NodeMock()
        {
            Id = SymbolIdBuilder.Execute(assemblySymbol),
            Symbol = assemblySymbol,
            SyntaxLinkList = [Utils.CreateAssemblySyntaxLink(name)]
        };

        parent?.ChildList.Add(node);

        return node;
    }

    public static INamespaceSymbol CreateNamespaceSymbol(string name, ISymbol? parent = null)
    {
        var symbol = parent
            ?? CreateAssemblySymbol("_");

        if (symbol is not IAssemblySymbol && symbol is not INamespaceSymbol)
        {
            throw new InvalidOperationException();
        }

        var assemblySymbol = symbol as IAssemblySymbol
            ?? symbol.ContainingAssembly
            ?? throw new InvalidOperationException();

        var moduleSymbol = assemblySymbol.Modules.First();

        var namespaceSymbolMock = Substitute.For<INamespaceSymbol>();
        namespaceSymbolMock.Name.Returns(name);
        namespaceSymbolMock.Kind.Returns(SymbolKind.Namespace);
        namespaceSymbolMock.ContainingModule.Returns(moduleSymbol);
        namespaceSymbolMock.ContainingAssembly.Returns(assemblySymbol);
        namespaceSymbolMock.ToDisplayString().Returns(name);

        return namespaceSymbolMock;
    }

    public static NodeMock CreateNamespaceNode(string name, NodeMock? parent)
    {
        var symbol = parent?.Symbol
            ?? CreateAssemblySymbol("_");

        if (symbol is not IAssemblySymbol && symbol is not INamespaceSymbol)
        {
            throw new InvalidOperationException();
        }

        var assemblySymbol = symbol as IAssemblySymbol
            ?? symbol.ContainingAssembly
            ?? throw new InvalidOperationException();

        var moduleSymbol = assemblySymbol.Modules.First();

        var namespaceSymbol = Substitute.For<INamespaceSymbol>();
        namespaceSymbol.Name.Returns(name);
        namespaceSymbol.Kind.Returns(SymbolKind.Namespace);
        namespaceSymbol.ContainingModule.Returns(moduleSymbol);
        namespaceSymbol.ContainingAssembly.Returns(assemblySymbol);
        namespaceSymbol.ToDisplayString().Returns(name);

        var node = new NodeMock()
        {
            Id = SymbolIdBuilder.Execute(namespaceSymbol),
            Symbol = namespaceSymbol
        };

        parent?.ChildList.Add(node);

        return node;
    }
}