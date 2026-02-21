using Microsoft.CodeAnalysis;
using NUnit.Framework;
using CSharpDepsGraph.Transforming.Filtering;
using CSharpDepsGraph.Tests.Syntax;

namespace CSharpDepsGraph.Tests.Transformation.Filtering;

// todo надо проверить после фильтрации не получаются ссылки на корень при удалении сборок
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

        var graph = ExecuteTransformer(originalGraph, $"**/N1", $"**/Foo2");
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

        var graph = ExecuteTransformer(originalGraph, FilterAction.Hide, $"**/N1/Foo1");
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

        var graph = ExecuteTransformer(originalGraph, FilterAction.Dissolve, $"**/N1/Foo1");

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

        var graph = ExecuteTransformer(originalGraph, $"**/Foo");

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

        var graph = ExecuteTransformer(originalGraph, $"**/F*oo1", $"**/Foo*2");

        Assert.That(graph.GetNode("N1").Childs.Count(), Is.EqualTo(1));
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