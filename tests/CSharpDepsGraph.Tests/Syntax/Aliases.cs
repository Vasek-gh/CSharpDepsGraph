using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Syntax;

public class Aliases : BaseSyntaxTests
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
}