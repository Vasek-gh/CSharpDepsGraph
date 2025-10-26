using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Syntax;

public class TypeDeclaration : BaseTests
{
    [Test]
    public void BaseParsed()
    {
        var graph = Build(@"
            using System;
            using TestProject.Entities;
            public class Test : Car, IDisposable, IComparable<Test> {
                public void Dispose() {}
                public int CompareTo(Test? other) { return 0; }
            }
        ");

        GraphAssert.HasLink(graph, "Test",
            (AsmName.TestProject, "TestProject.Entities.Car"),
            (AsmName.CoreLib, "System.IDisposable"),
            (AsmName.CoreLib, "System.IComparable<T>"),
            (AsmName.Test, "Test")
        );
    }

    [Test]
    public void GenericConstraintParsed()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test<T> where T : Vehicle {}
        ");

        GraphAssert.HasLink(graph, "Test<T>",
            (AsmName.TestProject, "TestProject.Entities.Vehicle")
        );
    }

    [Test]
    public void SymbolMapHaveClass()
    {
        var graph = Build(@"
            public class TestClass {
                public void Method() {}
            }
        ");

        GraphAssert.HasSymbol(graph, "TestClass");
        GraphAssert.HasSymbol(graph, "TestClass.Method()");
    }

    [Test]
    public void SymbolMapHaveInterface()
    {
        var graph = Build(@"
            public interface ITest {
                public void Method();
            }
        ");

        GraphAssert.HasSymbol(graph, "ITest");
        GraphAssert.HasSymbol(graph, "ITest.Method()");
    }

    [Test]
    public void SymbolMapHaveStruct()
    {
        var graph = Build(@"
            public struct Test {
                public void Method() {}
            }
        ");

        GraphAssert.HasSymbol(graph, "Test");
        GraphAssert.HasSymbol(graph, "Test.Method()");
    }

    [Test]
    public void SymbolMapHaveInnerClass()
    {
        var graph = Build(@"
            public class TestClass {
                public class InnerClass {
                    public void Method() {}
                }
            }
        ");

        GraphAssert.HasSymbol(graph, "TestClass");
        GraphAssert.HasSymbol(graph, "TestClass.InnerClass");
        GraphAssert.HasSymbol(graph, "TestClass.InnerClass.Method()");
    }

    [Test]
    public void EnumParsed()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public enum Test : byte {
                A = 1,
                B = 2,
            }
            public class SomeClass {
                public void Method() {
                    var a = Test.B;
                }
            }
        ");

        GraphAssert.HasSymbol(graph, "Test.A");
        GraphAssert.HasSymbol(graph, "Test.B");

        GraphAssert.HasLink(graph, "Test",
            (AsmName.CoreLib, "System.byte")
        );

        GraphAssert.HasLink(graph, "SomeClass.Method()",
            (AsmName.Test, "Test"),
            (AsmName.Test, "Test.B")
        );
    }

    [Test]
    public void DelegateParsed()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public delegate void Test(object x, int y);
        ");

        GraphAssert.HasLink(graph, "Test",
            (AsmName.CoreLib, "System.object"),
            (AsmName.CoreLib, "System.int")
        );
    }
}