using Microsoft.CodeAnalysis;
using Moq;
using NSubstitute;

namespace CSharpDepsGraph.Tests.Transformation;

internal static class Mocks
{
    public static IGraph CreateGraph(IEnumerable<INode> nodes, IEnumerable<ILink> links)
    {
        var rootMock = new Mock<INode>();
        rootMock.Setup(m => m.Uid).Returns(GraphConsts.RootNodeId);
        rootMock.Setup(m => m.Childs).Returns(nodes);
        rootMock.Setup(m => m.SyntaxLinks).Returns([]);

        var graphMock = new Mock<IGraph>();
        graphMock.Setup(m => m.Root).Returns(rootMock.Object);
        graphMock.Setup(m => m.Links).Returns(links);

        return graphMock.Object;
    }

    public static ILink CreateLink(INode source, INode target)
    {
        var syntaxLinkMock = Substitute.For<ILinkSyntaxLink>();
        syntaxLinkMock.LocationKind.Returns(LocationKind.Regular);

        var mock = Substitute.For<ILink>();
        mock.Source.Returns(source);
        mock.OriginalSource.Returns(source);
        mock.Target.Returns(target);
        mock.OriginalTarget.Returns(target);
        mock.SyntaxLink.Returns(syntaxLinkMock);

        return mock;
    }

    public static IAssemblySymbol CreateAssemblySymbol(string name, Version? version)
    {
        var assemblySymbolMock = Substitute.For<IAssemblySymbol>();
        var moduleSymbolMock = Substitute.For<IModuleSymbol>();
        var globaNamespaceSymbolMock = Substitute.For<INamespaceSymbol>();

        var assemblyIdentity = new AssemblyIdentity(name: name, version: version);

        assemblySymbolMock.Name.Returns(name);
        assemblySymbolMock.Kind.Returns(SymbolKind.Assembly);
        assemblySymbolMock.Identity.Returns(assemblyIdentity);
        assemblySymbolMock.Modules.Returns<IEnumerable<IModuleSymbol>>(new[] { moduleSymbolMock });
        assemblySymbolMock.GlobalNamespace.Returns(globaNamespaceSymbolMock);
        assemblySymbolMock.ToDisplayString().Returns(name);

        moduleSymbolMock.Name.Returns($"{name}.mdl");
        moduleSymbolMock.Kind.Returns(SymbolKind.NetModule);
        moduleSymbolMock.ContainingAssembly.Returns(assemblySymbolMock);
        moduleSymbolMock.GlobalNamespace.Returns(globaNamespaceSymbolMock);

        InitNamespaceSymbol(globaNamespaceSymbolMock, "", assemblySymbolMock, moduleSymbolMock, null);

        return assemblySymbolMock;
    }

    public static INamespaceSymbol CreateNamespaceSymbol(string name, ISymbol? parent = null)
    {
        var symbol = parent ?? CreateAssemblySymbol(name, null);
        if (symbol is not IAssemblySymbol && symbol is not INamespaceSymbol)
        {
            throw new InvalidOperationException();
        }

        var assemblySymbol = symbol as IAssemblySymbol
            ?? symbol.ContainingAssembly
            ?? throw new InvalidOperationException();

        var moduleSymbol = assemblySymbol.Modules.First();

        var result = Substitute.For<INamespaceSymbol>();
        InitNamespaceSymbol(result, name, assemblySymbol, moduleSymbol, assemblySymbol.GlobalNamespace);

        return result;
    }

    private static void InitNamespaceSymbol(
        INamespaceSymbol namespaceSymbol,
        string name,
        IAssemblySymbol assemblySymbol,
        IModuleSymbol moduleSymbol,
        INamespaceSymbol? parentNamespaceSymbol
        )
    {
        var isGlobal = parentNamespaceSymbol is null;

        name = !isGlobal ? name : $"{assemblySymbol.Name}.global::";

        namespaceSymbol.Name.Returns(name);
        namespaceSymbol.Kind.Returns(SymbolKind.Namespace);
        namespaceSymbol.ContainingAssembly.Returns(assemblySymbol);
        namespaceSymbol.ContainingModule.Returns(moduleSymbol);
        namespaceSymbol.ContainingNamespace.Returns(parentNamespaceSymbol);
        namespaceSymbol.IsGlobalNamespace.Returns(isGlobal);

        namespaceSymbol.ToDisplayString().Returns(name);
    }
}