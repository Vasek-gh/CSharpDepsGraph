using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Syntax;

public class Names : BaseSyntaxTests
{
    [Test]
    public void UsingAlias()
    {
        var graph = Build(@"
            using System;
            using TypeAlias = System.Action<int>;

            public class Test {
                public void Method(TypeAlias arg) {
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method(Action<int>)",
            (AsmName.CoreLib, "System/Action<T>")
        );
    }

    [Test]
    public void NamespaceHierarchy()
    {
        var graph = Build(@"
            namespace N1 {
                namespace N2 {
                    public class Foo {}
                }
            }
            namespace N1 {
                namespace N2.N3.N4 {
                    public class Bar {}
                }
            }
            namespace N1.N5 {
                namespace N2 {
                    public class Foo {}
                }
            }
            namespace N1.N5 {
                namespace N2.N3.N4 {
                    public class Bar {}
                }
            }
        ");

        var n1Node = graph.GetNode("N1");
        Assert.That(n1Node.Childs.Count(), Is.EqualTo(2)); // N2, N5

        // N2 branch

        var n12Node = n1Node.GetNode("N2");
        Assert.That(n12Node.Childs.Count(), Is.EqualTo(2)); // Foo, N3
        var n123Node = n12Node.GetNode("N3");
        Assert.That(n123Node.Childs.Count(), Is.EqualTo(1)); // N4
        var n1234Node = n123Node.GetNode("N4");
        Assert.That(n1234Node.Childs.Count(), Is.EqualTo(1)); // Bar

        var c12Node = n12Node.GetNode("Foo");
        var c1234Node = n1234Node.GetNode("Bar");

        // N5 branch

        var n15Node = n1Node.GetNode("N5");
        Assert.That(n15Node.Childs.Count(), Is.EqualTo(1)); // N2

        var n152Node = n15Node.GetNode("N2");
        Assert.That(n152Node.Childs.Count(), Is.EqualTo(2)); // Foo, N3
        var n1523Node = n152Node.GetNode("N3");
        Assert.That(n1523Node.Childs.Count(), Is.EqualTo(1)); // N4
        var n15234Node = n1523Node.GetNode("N4");
        Assert.That(n15234Node.Childs.Count(), Is.EqualTo(1)); // Bar

        var c152Node = n152Node.GetNode("Foo");
        var c15234Node = n15234Node.GetNode("Bar");
    }
}