using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Misc;

public class Aliases : BaseTests
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

        GraphAssert.HasLink(graph, "Test.Method(System.Action<int>)",
            (AsmName.CoreLib, "System.Action<T>")
        );
    }
}