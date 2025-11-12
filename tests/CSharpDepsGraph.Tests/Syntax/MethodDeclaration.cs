using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Syntax;

public class MethodDeclaration : BaseTests
{
    [Test]
    public void SymbolMapHaveMethod()
    {
        var graph = Build(@"
            public class Test {
                public void TestMethod() {}
            }
        ");

        GraphAssert.HasSymbol(graph, "Test.TestMethod()");
    }

    [Test]
    public void SymbolMapHaveConstructor()
    {
        var graph = Build(@"
            public class Test {
                Test() {}
            }
        ");

        GraphAssert.HasSymbol(graph, "Test.ctor()");
    }

    [Test]
    public void SymbolMapHaveStaticConstructor()
    {
        var graph = Build(@"
            public class Test {
                static Test() {}
            }
        ");

        GraphAssert.HasSymbol(graph, "Test.cctor()");
    }

    [Test]
    public void SymbolMapHaveDestructor()
    {
        var graph = Build(@"
            public class Test {
                ~Test() {}
            }
        ");

        GraphAssert.HasSymbol(graph, "Test.~()");
    }

    [Test]
    public void SymbolMapHaveMethodOverload()
    {
        var graph = Build(@"
            public class Test {
                public void TestMethod() {}
                public void TestMethod(int arg1) {}
                public void TestMethod(string arg1) {}
                public void TestMethod(int arg1, string arg) {}
            }
        ");

        GraphAssert.HasSymbol(graph, "Test.TestMethod()");
        GraphAssert.HasSymbol(graph, "Test.TestMethod(int)");
        GraphAssert.HasSymbol(graph, "Test.TestMethod(string)");
        GraphAssert.HasSymbol(graph, "Test.TestMethod(int,string)");

        Assert.Fail("Надо проверить что перегружаются так генерик методы");
    }

    [Test]
    public void ExplicitInterfaceParsed()
    {
        var graph = Build(@"
            public interface ITest {
                string TestMethod();
            }
            public class Test : ITest  {
                string ITest.TestMethod() { return string.Empty; }
            }
        ");

        GraphAssert.HasLink(graph, "Test.[ITest.TestMethod()]",
            (AsmName.CoreLib, "System.string"),
            (AsmName.Test, "ITest")
        );
    }

    [Test]
    public void ConstructorBaseParsed()
    {
        var graph = Build(@"
            using TestProject;
            public class Test : BaseTest {
                Test() : base(Statics.StrStatic1) {}
            }
            public class BaseTest {
                public BaseTest(string a) {}
            }
        ");

        GraphAssert.HasLink(graph, "Test.ctor()",
            (AsmName.TestProject, "TestProject.Statics"),
            (AsmName.TestProject, "TestProject.Statics.StrStatic1")
        );
    }

    [Test]
    public void RecordConstructors()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public record Test(Car Car, Size Size)
            {
                public Airplane Airplane { get; set; }
                public Test(Car car) : this(car, new Size()) {}
            }
        ");

        GraphAssert.HasLink(graph, "Test.ctor(TestProject.Entities.Car)",
            (AsmName.TestProject, "TestProject.Entities.Car")
        );

        GraphAssert.HasLink(graph, "Test.ctor(TestProject.Entities.Car,TestProject.Entities.Size)",
            (AsmName.TestProject, "TestProject.Entities.Car"),
            (AsmName.TestProject, "TestProject.Entities.Size")
        );

        GraphAssert.HasNotLink(graph, "Test",
            (AsmName.TestProject, "TestProject.Entities.Car"),
            (AsmName.TestProject, "TestProject.Entities.Size")
        );
    }

    [Test]
    public void ArgumentDefaults()
    {
        var graph = Build(@"
            using System.Threading;
            using TestProject;
            public class Test {
                public void TestMethod(int arg1 = Constants.IntConst1, CancellationToken cancellationToken = default(CancellationToken)) {}
            }
        ");

        GraphAssert.HasLink(graph, "Test.TestMethod(int,System.Threading.CancellationToken)",
            (AsmName.TestProject, "TestProject.Constants.IntConst1"),
            (AsmName.CoreLib, "System.Threading.CancellationToken")
        );
    }

    [Test]
    public void ArgumentAndReturnTypeParsed()
    {
        var graph = Build(@"
            using System.Threading;
            using System.Threading.Tasks;
            using TestProject.Entities;
            public class Test {
                public void TestMethod1(int arg) {}
                public void TestMethod2(CancellationToken cancellationToken) {}
                public void TestMethod3(Task<Car> arg) {}
                public CancellationToken TestMethod4() { return default; }
            }
        ");

        GraphAssert.HasLink(graph, "Test.TestMethod1(int)",
            (AsmName.CoreLib, "System.int")
        );

        GraphAssert.HasLink(graph, "Test.TestMethod2(System.Threading.CancellationToken)",
            (AsmName.CoreLib, "System.Threading.CancellationToken")
        );

        GraphAssert.HasLink(graph, "Test.TestMethod3(System.Threading.Tasks.Task<TestProject.Entities.Car>)",
            (AsmName.CoreLib, "System.Threading.Tasks.Task<TResult>"),
            (AsmName.TestProject, "TestProject.Entities.Car")
        );

        GraphAssert.HasLink(graph, "Test.TestMethod4()",
            (AsmName.CoreLib, "System.Threading.CancellationToken")
        );
    }

    [Test]
    public void TrivialBody()
    {
        var graph = Build(@"
            using System.Threading;
            public class Test {
                public CancellationToken TestMethod() { return new System.Threading.CancellationToken(); }
            }
        ");

        GraphAssert.HasLink(graph, "Test.TestMethod()",
            (AsmName.CoreLib, "System.Threading.CancellationToken")
        );
    }

    [Test]
    public void TrivialExpressionBody()
    {
        var graph = Build(@"
            using System.Threading;
            public class Test {
                public CancellationToken TestMethod() => new CancellationToken();
            }
        ");

        GraphAssert.HasLink(graph, "Test.TestMethod()",
            (AsmName.CoreLib, "System.Threading.CancellationToken")
        );
    }

    [Test]
    public void GenericConstraintParsed()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test {
                public void TestMethod<T>() where T : Vehicle {}
            }
        ");

        GraphAssert.HasLink(graph, "Test.TestMethod<T>()",
            (AsmName.TestProject, "TestProject.Entities.Vehicle")
        );
    }

    [Test]
    public void LocalFunctionPropagateSymbols()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test {
                public void TestMethod() {
                    Vehicle InnerMethod<T>(Car car) where T : Size {
                        return new Airplane();
                    }
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test.TestMethod()",
            (AsmName.TestProject, "TestProject.Entities.Car"),
            (AsmName.TestProject, "TestProject.Entities.Size"),
            (AsmName.TestProject, "TestProject.Entities.Vehicle"),
            (AsmName.TestProject, "TestProject.Entities.Airplane")
        );
    }
}