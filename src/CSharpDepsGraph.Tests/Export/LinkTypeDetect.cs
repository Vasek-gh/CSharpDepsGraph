using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CSharpDepsGraph.Export;

namespace CSharpDepsGraph.Tests.Export;

public class LinkTypeDetect : BaseTests
{
    [Test]
    public void ConstructorCall()
    {
        var graph = Build(@"
            using System.Threading;
            public class Test {
                public CancellationToken TestMethod() => new System.Threading.CancellationToken(true);
            }
        ");

        var ctorLinks = GetOutgoingLinks(graph, "Test.TestMethod()",
            (AsmName.CoreLib, "System.Threading.CancellationToken.CancellationToken(bool)")
        );

        Assert.That(ctorLinks.Single().GetLinkType() == LinkType.Call);

        var typeLinks = GetOutgoingLinks(graph, "Test.TestMethod()",
            (AsmName.CoreLib, "System.Threading.CancellationToken")
        );

        Assert.That(typeLinks.Single().GetLinkType() == LinkType.Reference);
    }

    [Test]
    public void ImplicitlyDeclaredConstructorCall()
    {
        var graph = Build(@"
            using System.Threading;
            public class Test {
                public CancellationToken TestMethod() => new System.Threading.CancellationToken();
            }
        ");

        var links = GetOutgoingLinks(graph, "Test.TestMethod()",
            (AsmName.CoreLib, "System.Threading.CancellationToken")
        );

        Assert.That(links.Count(), Is.EqualTo(2));
        Assert.That(links.Count(l => l.GetLinkType() == LinkType.Call), Is.EqualTo(1));
        Assert.That(links.Count(l => l.GetLinkType() == LinkType.Reference), Is.EqualTo(1));
    }

    [Test]
    public void MethodCall()
    {
        var graph = Build(@"
            using System.Threading;
            public class Test {
                public void TestMethod() {
                    var token = new System.Threading.CancellationToken();
                    token.ThrowIfCancellationRequested();
                }
            }
        ");

        var links = GetOutgoingLinks(graph, "Test.TestMethod()",
            (AsmName.CoreLib, "System.Threading.CancellationToken.ThrowIfCancellationRequested()")
        );

        Assert.That(links.Single().GetLinkType() == LinkType.Call);
    }

    [Test]
    public void ClassBaseList()
    {
        var graph = Build(@"
            class Test : System.Collections.ArrayList, System.IDisposable
            {
                public void Dispose() {}
            }
        ");

        var inheritsLinks = GetOutgoingLinks(graph, "Test",
            (AsmName.CoreLib, "System.Collections.ArrayList")
        );

        var implementsLinks = GetOutgoingLinks(graph, "Test",
            (AsmName.CoreLib, "System.IDisposable")
        );

        Assert.That(inheritsLinks.Single().GetLinkType() == LinkType.Inherits);
        Assert.That(implementsLinks.Single().GetLinkType() == LinkType.Implements);
    }

    [Test]
    public void InterfaceBaseList()
    {
        var graph = Build(@"
            interface ITest : System.IDisposable
            {
            }
        ");

        var inheritsLinks = GetOutgoingLinks(graph, "ITest",
            (AsmName.CoreLib, "System.IDisposable")
        );

        Assert.That(inheritsLinks.Single().GetLinkType() == LinkType.Inherits);
    }

    private static IEnumerable<ILink> GetOutgoingLinks(
        IGraph graph,
        string testAsmFullQualifiedName,
        (string assemblyName, string fullQualifiedName) targetName
        )
    {
        var source = graph.GetNode(AsmName.Test, testAsmFullQualifiedName);
        var target = graph.GetNode(targetName.assemblyName, targetName.fullQualifiedName);

        return graph.GetLinks(source, target).ToArray();
    }
}