using CSharpDepsGraph.Tests.Syntax;
using CSharpDepsGraph.Transforming.Filtering;
using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Transformation.Filtering;

public class FiltersTests : BaseSyntaxTests
{
    [Test]
    public void HidePrivate()
    {
        var originalGraph = Build(@"
            class Foo {
                public int _bar1;
                private int _bar2;

                public void Bar1() { _bar2 = 1; }
                private void Bar2() { _bar1 = 1; }

                public class InnerFoo1 {}
                private class InnerFoo2 {}
            }
        ");

        GraphAssert.HasSymbol(originalGraph, "Foo/_bar2");
        GraphAssert.HasSymbol(originalGraph, "Foo/Bar2()");
        GraphAssert.HasSymbol(originalGraph, "Foo/InnerFoo2");

        Assert.That(originalGraph.GetOutgoingLinks(originalGraph.GetNode("Foo/Bar1()")), Is.Not.Empty);
        Assert.That(originalGraph.GetIncomingLinks(originalGraph.GetNode("Foo/_bar1")), Is.Not.Empty);

        var graph = ExecuteTransformer(originalGraph, Filters.HidePrivate);

        GraphAssert.HasSymbol(graph, "Foo/InnerFoo1");
        GraphAssert.HasNotSymbol(graph, "Foo/Bar2()");
        GraphAssert.HasNotSymbol(graph, "Foo/InnerFoo2");

        Assert.That(graph.GetOutgoingLinks(graph.GetNode("Foo/Bar1()")), Is.Empty);
        Assert.That(graph.GetIncomingLinks(graph.GetNode("Foo/_bar1")), Is.Empty);
    }

    [Test]
    public void DissolveMembers()
    {
        var originalGraph = Build(@"
            class Foo {
                public int _bar;
                private void Bar() { }

                public class InnerFoo {}
            }
            class FooClient {
                public Foo _bar;
            }
        ");

        var link = originalGraph.GetOutgoingLinks(originalGraph.GetNode("FooClient/_bar")).Single();

        GraphAssert.HasSymbol(originalGraph, "Foo/_bar");
        GraphAssert.HasSymbol(originalGraph, "Foo/Bar()");
        GraphAssert.HasSymbol(originalGraph, "Foo/InnerFoo");
        GraphAssert.HasSymbol(originalGraph, "FooClient/_bar");

        var graph = ExecuteTransformer(originalGraph, Filters.DissolveMembers);

        GraphAssert.HasNotSymbol(graph, "Foo/_bar");
        GraphAssert.HasNotSymbol(graph, "Foo/Bar()");
        GraphAssert.HasSymbol(graph, "Foo/InnerFoo");
        GraphAssert.HasNotSymbol(graph, "FooClient/_bar");

        var newLink = graph.GetOutgoingLinks(graph.GetNode("FooClient")).Single();

        Assert.That(newLink.Source.Uid, Is.EqualTo(graph.GetNode("FooClient").Uid));
        Assert.That(newLink.Target.Uid, Is.EqualTo(graph.GetNode("Foo").Uid));
        Assert.That(newLink.OriginalSource.Uid, Is.EqualTo(originalGraph.GetNode("FooClient/_bar").Uid));
        Assert.That(newLink.OriginalTarget.Uid, Is.EqualTo(originalGraph.GetNode("Foo").Uid));
    }

    [Test]
    public void DissolveTypes()
    {
        var originalGraph = Build(@"
            namespace N1 {
                class Foo {}
            }

            namespace N2 {
                class FooClient {
                    public N1.Foo _bar;
                }
            }
        ");

        var link = originalGraph.GetOutgoingLinks(originalGraph.GetNode("N2/FooClient/_bar")).Single();

        GraphAssert.HasSymbol(originalGraph, "N1/Foo");
        GraphAssert.HasSymbol(originalGraph, "N2/FooClient");

        var graph = ExecuteTransformer(originalGraph, Filters.DissolveTypes);

        GraphAssert.HasNotSymbol(graph, "N1/Foo");
        GraphAssert.HasNotSymbol(graph, "N2/FooClient");

        var newLink = graph.GetOutgoingLinks(graph.GetNode("N2")).Single();

        Assert.That(newLink.Source.Uid, Is.EqualTo(graph.GetNode("N2").Uid));
        Assert.That(newLink.Target.Uid, Is.EqualTo(graph.GetNode("N1").Uid));
        Assert.That(newLink.OriginalSource.Uid, Is.EqualTo(originalGraph.GetNode("N2/FooClient/_bar").Uid));
        Assert.That(newLink.OriginalTarget.Uid, Is.EqualTo(originalGraph.GetNode("N1/Foo").Uid));
    }

    [Test]
    public void DissolveNamespaces()
    {
        var originalGraph = Build(@"
            namespace N1 {
                class Foo {
                    public int _bar;
                }
            }
        ");

        var link = originalGraph.GetOutgoingLinks(originalGraph.GetNode("N1/Foo/_bar")).Single();

        GraphAssert.HasSymbol(originalGraph, "N1");
        GraphAssert.HasSymbol(originalGraph, (AsmName.CoreLib, "System/int"));

        var graph = ExecuteTransformer(originalGraph, Filters.DissolveNamespaces);

        GraphAssert.HasNotSymbol(graph, "N1");
        GraphAssert.HasNotSymbol(graph, (AsmName.CoreLib, "System/int"));

        var newLink = graph.GetOutgoingLinks(graph.GetNode("")).Single();

        Assert.That(newLink.Source.Uid, Is.EqualTo(graph.GetNode("").Uid));
        Assert.That(newLink.Target.Uid, Is.EqualTo(graph.GetNode(AsmName.CoreLib, "").Uid));
        Assert.That(newLink.OriginalSource.Uid, Is.EqualTo(originalGraph.GetNode("N1/Foo/_bar").Uid));
        Assert.That(newLink.OriginalTarget.Uid, Is.EqualTo(originalGraph.GetNode(AsmName.CoreLib, "System/int").Uid));
    }

    private static IGraph ExecuteTransformer(IGraph sourceGraph, INodeFilter filter)
    {
        return new FilterTransformer(filter).Execute(sourceGraph);
    }
}