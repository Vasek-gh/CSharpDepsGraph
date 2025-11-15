using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Syntax;

public class MethodDeclaration : BaseSyntaxTests
{
    [Test]
    public void NodeHaveMethod()
    {
        var graph = Build(@"
            public class Test {
                public void TestMethod() {}
            }
        ");

        GraphAssert.HasSymbol(graph, "Test/TestMethod()");
    }

    [Test]
    public void NodeHaveConstructor()
    {
        var graph = Build(@"
            public class Test {
                Test() {}
            }
        ");

        GraphAssert.HasSymbol(graph, "Test/ctor()");
    }

    [Test]
    public void NodeHaveStaticConstructor()
    {
        var graph = Build(@"
            public class Test {
                static Test() {}
            }
        ");

        GraphAssert.HasSymbol(graph, "Test/sctor()");
    }

    [Test]
    public void NodeHaveDestructor()
    {
        var graph = Build(@"
            public class Test {
                ~Test() {}
            }
        ");

        GraphAssert.HasSymbol(graph, "Test/~Test()");
    }

    [Test]
    public void NodeHaveAllOverloads()
    {
        var graph = Build(@"
            public class Test {
                public void TestMethod() {}
                public void TestMethod(int arg1) {}
                public void TestMethod(string arg1) {}
                public void TestMethod(int arg1, string arg) {}
                public void TestMethod<T1>(T1 arg1) {}
                public void TestMethod<T1, T2>(T1 arg1, T2 arg2) {}
            }
        ");

        GraphAssert.HasSymbol(graph, "Test/TestMethod()");
        GraphAssert.HasSymbol(graph, "Test/TestMethod(int)");
        GraphAssert.HasSymbol(graph, "Test/TestMethod(string)");
        GraphAssert.HasSymbol(graph, "Test/TestMethod(int, string)");
        GraphAssert.HasSymbol(graph, "Test/TestMethod<T1>(T1)");
        GraphAssert.HasSymbol(graph, "Test/TestMethod<T1, T2>(T1, T2)");
    }

    [Test]
    public void ExplicitInterfaceParsed()
    {
        var graph = Build(@"
            public interface ITest {
                string TestMethod();
                string TestMethod<T>(T arg);
            }
            public interface ITest<T> {
                string TestMethod();
            }
            public class Test : ITest, ITest<int>  {
                string ITest.TestMethod() { return string.Empty; }
                string ITest.TestMethod<T>(T arg) { return string.Empty; }
                string ITest<int>.TestMethod() { return string.Empty; }
                string TestMethod() { return string.Empty; }
            }
        ");

        GraphAssert.HasLink(graph, "Test/ITest.TestMethod()",
            (AsmName.CoreLib, "System/string"),
            (AsmName.Test, "ITest")
        );

        GraphAssert.HasLink(graph, "Test/ITest.TestMethod<T>(T)",
            (AsmName.CoreLib, "System/string"),
            (AsmName.Test, "ITest")
        );

        GraphAssert.HasLink(graph, "Test/ITest<int>.TestMethod()",
            (AsmName.CoreLib, "System/string"),
            (AsmName.CoreLib, "System/int"),
            (AsmName.Test, "ITest<T>")
        );

        GraphAssert.HasLink(graph, "Test/TestMethod()",
            (AsmName.CoreLib, "System/string")
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

        GraphAssert.HasLink(graph, "Test/ctor()",
            (AsmName.TestProject, "TestProject/Statics"),
            (AsmName.TestProject, "TestProject/Statics/StrStatic1")
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

        GraphAssert.HasLink(graph, "Test/ctor(Car)",
            (AsmName.TestProject, "TestProject/Entities/Car")
        );

        GraphAssert.HasLink(graph, "Test/ctor(Car, Size)",
            (AsmName.TestProject, "TestProject/Entities/Car"),
            (AsmName.TestProject, "TestProject/Entities/Size")
        );

        GraphAssert.HasNotLink(graph, "Test",
            (AsmName.TestProject, "TestProject/Entities/Car"),
            (AsmName.TestProject, "TestProject/Entities/Size")
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

        GraphAssert.HasLink(graph, "Test/TestMethod(int, CancellationToken)",
            (AsmName.TestProject, "TestProject/Constants/IntConst1"),
            (AsmName.CoreLib, "System/Threading/CancellationToken")
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

        GraphAssert.HasLink(graph, "Test/TestMethod1(int)",
            (AsmName.CoreLib, "System/int")
        );

        GraphAssert.HasLink(graph, "Test/TestMethod2(CancellationToken)",
            (AsmName.CoreLib, "System/Threading/CancellationToken")
        );

        GraphAssert.HasLink(graph, "Test/TestMethod3(Task<Car>)",
            (AsmName.CoreLib, "System/Threading/Tasks/Task<TResult>"),
            (AsmName.TestProject, "TestProject/Entities/Car")
        );

        GraphAssert.HasLink(graph, "Test/TestMethod4()",
            (AsmName.CoreLib, "System/Threading/CancellationToken")
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

        GraphAssert.HasLink(graph, "Test/TestMethod()",
            (AsmName.CoreLib, "System/Threading/CancellationToken")
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

        GraphAssert.HasLink(graph, "Test/TestMethod()",
            (AsmName.CoreLib, "System/Threading/CancellationToken")
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

        GraphAssert.HasLink(graph, "Test/TestMethod<T>() where T : Vehicle",
            (AsmName.TestProject, "TestProject/Entities/Vehicle")
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

        GraphAssert.HasLink(graph, "Test/TestMethod()",
            (AsmName.TestProject, "TestProject/Entities/Car"),
            (AsmName.TestProject, "TestProject/Entities/Size"),
            (AsmName.TestProject, "TestProject/Entities/Vehicle"),
            (AsmName.TestProject, "TestProject/Entities/Airplane")
        );
    }
}