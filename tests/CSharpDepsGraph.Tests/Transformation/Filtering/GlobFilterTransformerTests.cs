using Microsoft.CodeAnalysis;
using NUnit.Framework;
using CSharpDepsGraph.Transforming.Filtering;
using CSharpDepsGraph.Tests.Syntax;
using CSharpDepsGraph.Transforming;

namespace CSharpDepsGraph.Tests.Transformation.Filtering;

// todo check what after filter done no one link pointing to root
// todo check what happen with links, when link what pointed to deleted node
[TestFixture]
public class GlobFilterTransformerTests : BaseSyntaxTests
{
    [Test]
    public void Trivial()
    {
        var originalGraph = Build(@"
            namespace N1 {
                public class Foo1 {}
            }
            namespace N2 {
                public class Foo2 {}
            }
        ");

        var graph = ExecuteTransformer(originalGraph, $"**/N1", $"**/*.Foo2");
        var asmNode = graph.GetNode("");

        Assert.That(asmNode.Childs.Count(), Is.EqualTo(1));
        Assert.That(asmNode.Childs.Single().Childs, Is.Empty);
    }

    [Test]
    public void Hide()
    {
        var originalGraph = Build(@"
            namespace N1 {
                public class Foo1 {}
            }
            namespace N2 {
                public class Foo2 : N1.Foo1 {}
            }
        ");

        GraphAssert.HasExactLink(originalGraph, "N2/Foo2",
            (AsmName.Test, "N1/Foo1")
        );

        var graph = ExecuteTransformer(originalGraph, FilterAction.Hide, $"**/N1.Foo1");
        var node = graph.GetNode("N2/Foo2");

        Assert.That(graph.GetOutgoingLinks(node), Is.Empty);
    }

    [Test]
    public void Dissolve()
    {
        var originalGraph = Build(@"
            namespace N1 {
                public class Foo1 {}
            }
            namespace N2 {
                public class Foo2 : N1.Foo1 {}
            }
        ");

        GraphAssert.HasExactLink(originalGraph, "N2/Foo2",
            (AsmName.Test, "N1/Foo1")
        );

        var graph = ExecuteTransformer(originalGraph, FilterAction.Dissolve, $"**/N1.Foo1");

        GraphAssert.HasExactLink(graph, "N2/Foo2",
            (AsmName.Test, "N1")
        );
    }

    [Test]
    public void SameName()
    {
        var originalGraph = Build(@"
            namespace N1 {
                public class Foo {}
            }
            namespace N2 {
                public class Foo {}
            }
            namespace N3 {
                public class Foo1 {}
            }
        ");

        var graph = ExecuteTransformer(originalGraph, $"**/*.Foo");

        Assert.That(graph.GetNode("N1").Childs, Is.Empty);
        Assert.That(graph.GetNode("N2").Childs, Is.Empty);
        Assert.That(graph.GetNode("N3").Childs.Count(), Is.EqualTo(1));
    }

    [Test]
    public void Wildcards()
    {
        var originalGraph = Build(@"
            namespace N1 {
                public class F_oo1 {}
                public class Foo__2 {}
                public class Bar {}
            }
        ");

        var graph = ExecuteTransformer(originalGraph, $"**/*.F*oo1", $"**/*.Foo*2");

        Assert.That(graph.GetNode("N1").Childs.Count(), Is.EqualTo(1));
    }

    [Test]
    public void AssemblyFilter()
    {
        var originalGraph = Build(@"
            namespace For {
                public class Bar {}
            }
        ");

        GraphAssert.HasSymbol(originalGraph, (AsmName.CoreLib, null));

        GraphAssert.HasNotSymbol(
            ExecuteTransformer(originalGraph, "System*"),
            (AsmName.CoreLib, null)
            );

        GraphAssert.HasNotSymbol(
            ExecuteTransformer(originalGraph, "System.*"),
            (AsmName.CoreLib, null)
            );

        GraphAssert.HasNotSymbol(
            ExecuteTransformer(originalGraph, "**/System*"),
            (AsmName.CoreLib, null)
            );

        GraphAssert.HasNotSymbol(
            ExecuteTransformer(originalGraph, "**/System.*"),
            (AsmName.CoreLib, null)
            );
    }

    [Test]
    [Description("Tests the filter on a raw graph")]
    public void NamespaceFilterHierarchy()
    {
        var originalGraph = Build(@"
            namespace N1 {
                class Foo {}
            }
            namespace N1.N2 {
                class Foo {}
            }
            namespace N3 {
                class Foo {}
            }
        ");

        GraphAssert.HasSymbol(originalGraph, "N1");
        GraphAssert.HasSymbol(originalGraph, "N1/Foo");
        GraphAssert.HasSymbol(originalGraph, "N1/N2");
        GraphAssert.HasSymbol(originalGraph, "N1/N2/Foo");
        GraphAssert.HasSymbol(originalGraph, "N3");
        GraphAssert.HasSymbol(originalGraph, "N3/Foo");

        var graph1 = ExecuteTransformer(originalGraph, "**/N1*");
        GraphAssert.HasNotSymbol(graph1, "N1");
        GraphAssert.HasNotSymbol(graph1, "N1/Foo");
        GraphAssert.HasNotSymbol(graph1, "N1/N2");
        GraphAssert.HasNotSymbol(graph1, "N1/N2/Foo");
        GraphAssert.HasSymbol(graph1, "N3/Foo");
        Assert.That(graph1.GetNode(AsmName.Test, null).Childs.Count(), Is.EqualTo(1));

        var graph2 = ExecuteTransformer(originalGraph, "**/N1.*");
        GraphAssert.HasSymbol(graph2, "N1");
        GraphAssert.HasNotSymbol(graph2, "N1/Foo");
        GraphAssert.HasNotSymbol(graph2, "N1/N2");
        GraphAssert.HasNotSymbol(graph2, "N1/N2/Foo");
        GraphAssert.HasSymbol(graph2, "N3/Foo");
        Assert.That(graph2.GetNode(AsmName.Test, null).Childs.Count(), Is.EqualTo(2));

        var graph3 = ExecuteTransformer(originalGraph, "**/N1.N2*");
        GraphAssert.HasSymbol(graph3, "N1");
        GraphAssert.HasSymbol(graph3, "N1/Foo");
        GraphAssert.HasNotSymbol(graph3, "N1/N2");
        GraphAssert.HasNotSymbol(graph3, "N1/N2/Foo");
        GraphAssert.HasSymbol(graph3, "N3/Foo");
        Assert.That(graph3.GetNode(AsmName.Test, null).Childs.Count(), Is.EqualTo(2));

        var graph4 = ExecuteTransformer(originalGraph, "**/N1.N2.*");
        GraphAssert.HasSymbol(graph4, "N1");
        GraphAssert.HasSymbol(graph4, "N1/Foo");
        GraphAssert.HasSymbol(graph4, "N1/N2");
        GraphAssert.HasNotSymbol(graph4, "N1/N2/Foo");
        GraphAssert.HasSymbol(graph4, "N3/Foo");
        Assert.That(graph4.GetNode(AsmName.Test, null).Childs.Count(), Is.EqualTo(2));
    }

    [Test]
    [Description("Tests the filter on the graph after applying FlattenNamespacesTransformer")]
    public void NamespaceFilterFlatten()
    {
        var originalGraph = Build(@"
            namespace N1 {
                class Foo {}
            }
            namespace N1.N2 {
                class Foo {}
            }
            namespace N3 {
                class Foo {}
            }
        ");

        originalGraph = new FlattenNamespacesTransformer().Execute(originalGraph);
        GraphAssert.HasSymbol(originalGraph, "N1");
        GraphAssert.HasSymbol(originalGraph, "N1/Foo");
        GraphAssert.HasSymbol(originalGraph, "N2");
        GraphAssert.HasSymbol(originalGraph, "N2/Foo");
        GraphAssert.HasSymbol(originalGraph, "N3");
        GraphAssert.HasSymbol(originalGraph, "N3/Foo");

        var graph1 = ExecuteTransformer(originalGraph, "**/N1*");
        GraphAssert.HasNotSymbol(graph1, "N1");
        GraphAssert.HasNotSymbol(graph1, "N1/Foo");
        GraphAssert.HasNotSymbol(graph1, "N1/N2");
        GraphAssert.HasNotSymbol(graph1, "N1/N2/Foo");
        GraphAssert.HasSymbol(graph1, "N3/Foo");
        Assert.That(graph1.GetNode(AsmName.Test, null).Childs.Count(), Is.EqualTo(1));

        var graph2 = ExecuteTransformer(originalGraph, "**/N1.*");
        GraphAssert.HasSymbol(graph2, "N1");
        GraphAssert.HasNotSymbol(graph2, "N1/Foo");
        GraphAssert.HasNotSymbol(graph2, "N1/N2");
        GraphAssert.HasNotSymbol(graph2, "N1/N2/Foo");
        GraphAssert.HasSymbol(graph2, "N3/Foo");
        Assert.That(graph2.GetNode(AsmName.Test, null).Childs.Count(), Is.EqualTo(2));

        var graph3 = ExecuteTransformer(originalGraph, "**/N1.N2*");
        GraphAssert.HasSymbol(graph3, "N1");
        GraphAssert.HasSymbol(graph3, "N1/Foo");
        GraphAssert.HasNotSymbol(graph3, "N1/N2");
        GraphAssert.HasNotSymbol(graph3, "N1/N2/Foo");
        GraphAssert.HasSymbol(graph3, "N3/Foo");
        Assert.That(graph3.GetNode(AsmName.Test, null).Childs.Count(), Is.EqualTo(2));

        var graph4 = ExecuteTransformer(originalGraph, "**/N1.N2.*");
        GraphAssert.HasSymbol(graph4, "N1");
        GraphAssert.HasSymbol(graph4, "N1/Foo");
        GraphAssert.HasSymbol(graph4, "N2");
        GraphAssert.HasNotSymbol(graph4, "N1/N2/Foo");
        GraphAssert.HasSymbol(graph4, "N3/Foo");
        Assert.That(graph4.GetNode(AsmName.Test, null).Childs.Count(), Is.EqualTo(3));
    }

    [Test]
    [Description("Tests the filter on the graph after applying FlattenNamespacesTransformer+NamespaceOnlyTransformer")]
    public void NamespaceFilterOnlyNamespaces()
    {
        var originalGraph = Build(@"
            namespace N1 {
                class Foo {}
            }
            namespace N1.N2 {
                class Foo {}
            }
            namespace N3 {
                class Foo {}
            }
        ");

        originalGraph = new FlattenNamespacesTransformer().Execute(originalGraph);
        originalGraph = new NamespaceOnlyTransformer().Execute(originalGraph);
        GraphAssert.HasSymbol(originalGraph, ("N1", null));
        GraphAssert.HasSymbol(originalGraph, ("N2", null));
        GraphAssert.HasSymbol(originalGraph, ("N3", null));

        var graph1 = ExecuteTransformer(originalGraph, "N1*");
        GraphAssert.HasNotSymbol(graph1, ("N1", null));
        GraphAssert.HasNotSymbol(graph1, ("N2", null));
        GraphAssert.HasSymbol(graph1, ("N3", null));

        var graph2 = ExecuteTransformer(originalGraph, "N1.*");
        GraphAssert.HasSymbol(graph2, ("N1", null));
        GraphAssert.HasNotSymbol(graph2, ("N2", null));
        GraphAssert.HasSymbol(graph2, ("N3", null));

        var graph3 = ExecuteTransformer(originalGraph, "N1.N2*");
        GraphAssert.HasSymbol(graph3, ("N1", null));
        GraphAssert.HasNotSymbol(graph3, ("N2", null));
        GraphAssert.HasSymbol(graph3, ("N3", null));

        var graph4 = ExecuteTransformer(originalGraph, "N1.N2.*");
        GraphAssert.HasSymbol(graph4, ("N1", null));
        GraphAssert.HasSymbol(graph4, ("N2", null));
        GraphAssert.HasSymbol(graph4, ("N3", null));
    }

    private static IGraph ExecuteTransformer(IGraph sourceGraph, FilterAction action, string pattern)
    {
        var filter = new GlobFilter(action, pattern);

        return new FilterTransformer(filter).Execute(sourceGraph);
    }

    private static IGraph ExecuteTransformer(IGraph sourceGraph, params string[] patterns)
    {
        var filters = patterns.Select(p => new GlobFilter(FilterAction.Hide, p));

        return new FilterTransformer(filters).Execute(sourceGraph);
    }
}