using CSharpDepsGraph;
using Moq;
using System.Collections.Generic;

namespace CSharpDepsGraph.Tests.Mutation;

public static class Mocks
{
    public static IGraph CreateGraph(INode root, IEnumerable<ILink> links)
    {
        var mock = new Mock<IGraph>();
        mock.Setup(m => m.Root).Returns(root);
        mock.Setup(m => m.Links).Returns(links);

        return mock.Object;
    }

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
        var mock = new Mock<ILink>();
        mock.Setup(m => m.Source).Returns(source);
        mock.Setup(m => m.OriginalSource).Returns(source);
        mock.Setup(m => m.Target).Returns(target);
        mock.Setup(m => m.OriginalTarget).Returns(target);

        mock.Setup(m => m.SyntaxLink).Returns(new SyntaxLink()
        {
            FileKind = SyntaxFileKind.Local,
            Path = ""
        });

        return mock.Object;
    }
}