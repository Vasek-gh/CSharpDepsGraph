using NUnit.Framework;
using CSharpDepsGraph.Transforming;
using CSharpDepsGraph.Tests.Syntax;

namespace CSharpDepsGraph.Tests.Transformation;

[TestFixture]
public class ExternalHideTransformerTests : BaseSyntaxTests
{
    [Test]
    public void Trivial()
    {
        var graph = Build(@"
            using System;
            class Foo1 {}
            class Foo2 {
                void Bar(int arg1, Foo1 arg2) {}
            }
        ");

        GraphAssert.HasSymbol(graph, (AsmName.CoreLib, "System"));
        GraphAssert.HasExactLink(graph, "Foo2/Bar(int, Foo1)",
            (AsmName.CoreLib, "System/int"),
            (AsmName.Test, "Foo1")
        );

        var transformedGraph = new MetadataHideTransformer().Execute(graph);

        GraphAssert.HasNotSymbol(transformedGraph, (AsmName.CoreLib, "System"));
        GraphAssert.HasExactLink(transformedGraph, "Foo2/Bar(int, Foo1)",
            (AsmName.Test, "Foo1")
        );
    }
}