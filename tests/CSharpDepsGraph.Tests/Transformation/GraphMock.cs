using System.Collections.Generic;

namespace CSharpDepsGraph.Tests.Transformation;

internal class GraphMock : IGraph
{
    public NodeMock RootNode { get; } = new NodeMock()
    {
        Id = GraphConsts.RootNodeId
    };

    public INode Root => RootNode;

    public List<ILink> LinkList { get; } = [];

    public IEnumerable<ILink> Links => LinkList;

    public void AddLink(ILink link)
    {
        LinkList.Add(link);
    }
}