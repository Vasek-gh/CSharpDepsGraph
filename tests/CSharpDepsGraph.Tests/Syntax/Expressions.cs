using System.Linq;
using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Syntax;

#pragma warning disable CA1724
public class Expressions : BaseSyntaxTests //todo rename
#pragma warning restore CA2000
{
    [Test]
    public void ObjectCreationSimple()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test {
                public void Method() {
                    var obj1 = new Car();
                    var obj2 = new Car(new Size());
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method()",
            (AsmName.TestProject, "TestProject/Entities/Car/ctor()"),
            (AsmName.TestProject, "TestProject/Entities/Car/ctor(Size)"),
            (AsmName.TestProject, "TestProject/Entities/Size")
        );

        GraphAssert.HasNotLink(graph, "Test/Method()",
            (AsmName.TestProject, "TestProject"),
            (AsmName.TestProject, "TestProject/Entities"),
            (AsmName.TestProject, "TestProject/Entities/Car")
        );

        GraphAssert.HasNotSymbol(graph, (AsmName.TestProject, "TestProject/Entities/Size/Size()"));
    }

    [Test]
    public void ObjectCreationGenericSimple()
    {
        var graph = Build(@"
            using TestProject;
            using TestProject.Entities;
            public class Test {
                public void TestMethod() {
                    var obj1 = new GenericClass1<Car>();
                    var obj2 = new GenericClass1<Vehicle>(new Airplane());
                    var obj3 = new GenericClass2<Car>();
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/TestMethod()",
            (AsmName.TestProject, "TestProject/GenericClass1<T>/ctor()"),
            (AsmName.TestProject, "TestProject/GenericClass1<T>/ctor(T)"),
            (AsmName.TestProject, "TestProject/GenericClass2<T>"),
            (AsmName.TestProject, "TestProject/Entities/Car"),
            (AsmName.TestProject, "TestProject/Entities/Vehicle")
        );

        GraphAssert.HasNotLink(graph, "Test/TestMethod()",
            (AsmName.TestProject, "TestProject"),
            (AsmName.TestProject, "TestProject/Entities"),
            (AsmName.TestProject, "TestProject/GenericClass1<T>")
        );

        GraphAssert.HasNotSymbol(graph, (AsmName.TestProject, "TestProject/GenericClass2<T>/GenericClass2()"));
    }

    [Test]
    public void ObjectCreationQualified()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test {
                public void Method() {
                    var obj1 = new TestProject.Entities.Car();
                    var obj2 = new TestProject.Entities.Car(new Size());
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method()",
            (AsmName.TestProject, "TestProject/Entities/Car/ctor()"),
            (AsmName.TestProject, "TestProject/Entities/Car/ctor(Size)"),
            (AsmName.TestProject, "TestProject/Entities/Size")
        );

        GraphAssert.HasNotLink(graph, "Test/Method()",
            (AsmName.TestProject, "TestProject/Entities/Car")
        );

        GraphAssert.HasNotSymbol(graph, (AsmName.TestProject, "TestProject/Entities/Size/Size()"));
    }


    [Test]
    public void ObjectCreationGenericQualified()
    {
        var graph = Build(@"
            using TestProject;
            using TestProject.Entities;
            public class Test {
                public void TestMethod() {
                    var obj1 = new TestProject.GenericClass1<Car>();
                    var obj2 = new TestProject.GenericClass1<Vehicle>(new Airplane());
                    var obj3 = new TestProject.GenericClass2<Car>();
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/TestMethod()",
            (AsmName.TestProject, "TestProject/GenericClass1<T>/ctor()"),
            (AsmName.TestProject, "TestProject/GenericClass1<T>/ctor(T)"),
            (AsmName.TestProject, "TestProject/GenericClass2<T>"),
            (AsmName.TestProject, "TestProject/Entities/Car"),
            (AsmName.TestProject, "TestProject/Entities/Vehicle")
        );

        GraphAssert.HasNotLink(graph, "Test/TestMethod()",
            (AsmName.TestProject, "TestProject/GenericClass1<T>")
        );

        GraphAssert.HasNotSymbol(graph, (AsmName.TestProject, "TestProject/GenericClass2<T>/GenericClass2()"));
    }

    [Test]
    public void ObjectCreationInitializer()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test {
                public void Method() {
                    var obj = new Car {
                        Price = 1,
                        Size = new Size() {
                            Width = 10
                        }
                    };
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method()",
            (AsmName.TestProject, "TestProject/Entities/Car/ctor()"),
            (AsmName.TestProject, "TestProject/Entities/Vehicle/Price"),
            (AsmName.TestProject, "TestProject/Entities/Car/Size"),
            (AsmName.TestProject, "TestProject/Entities/Size"),
            (AsmName.TestProject, "TestProject/Entities/Size/Width")
        );
    }

    [Test]
    public void ObjectCreationCollectionInitializer()
    {
        var graph = Build(@"
            using System.Collections.Generic;
            using TestProject.Entities;
            public class Test  {
                public void Method() {
                    var items = new List<Vehicle>() {
                        new Car(),
                        new Airplane(),
                    };
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method()",
            (AsmName.CoreLib, "System/Collections/Generic/List<T>/ctor()"),
            (AsmName.TestProject, "TestProject/Entities/Vehicle"),
            (AsmName.TestProject, "TestProject/Entities/Car/ctor()"),
            (AsmName.TestProject, "TestProject/Entities/Airplane")
        );
    }

    [Test]
    public void ObjectCreationGenericSymbol()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test {
                public void Method<T>() where T : Vehicle, new() {
                    Vehicle obj1 = new T();
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method<T>() where T : Vehicle, new()",
            (AsmName.TestProject, "TestProject/Entities/Vehicle")
        );
    }

    [Test]
    public void ObjectCreationDelegate()
    {
        var graph = Build(@"
            using System;
            public class Test {
                public void Method() {
                    var action = new Action(() => { Console.WriteLine(""Action""); });
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method()",
            (AsmName.CoreLib, "System/Action"),
            ("External/System.Console_8.0.0.0", "System/Console")
        );
    }

    [Test]
    public void AnonymousObjectCreation()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test {
                public void Method(Car car) {
                    var obj = new {
                        Title = ""123"",
                        Cars = new [] { car, new Car() },
                        Qwerty = new {
                            Size = new Size()
                        }
                    };
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method(Car)",
            (AsmName.TestProject, "TestProject/Entities/Car"),
            (AsmName.TestProject, "TestProject/Entities/Car/ctor()"),
            (AsmName.TestProject, "TestProject/Entities/Size")
        );
    }

    [Test]
    public void AnonymousObjectMemberIgnored()
    {
        // If we do not ignore the reference to the anonymous object member, a reference
        // to it will be created, and a node will be created for the anonymous object. Anonymous
        // types are created directly in the assembly, so it is enough to check that the assembly
        // does not have this type

        var graph = Build(@"
            using System;
            using TestProject.Entities;
            public class Test {
                public void TestMethod() {
                    var v1 = new { AnonymousProp = 1 };
                    var v2 = v1.AnonymousProp;
                }
            }
        ");

        Assert.That(
            graph.GetNode(AsmName.Test)
                .Childs
                .Count(n => n.Id.Contains("AnonymousProp")),
            Is.EqualTo(0)
        );
    }

    [Test]
    public void MemberAccess()
    {
        var graph = Build(@"
            using System;
            using TestProject.Entities;
            public class Test {
                private SimpleClass f;

                public void Method() {

                    var q1 = f.F.Size;
                    var q2 = f.P.Size;
                    var q3 = f.M().Size;
                    f.E += (_) => {};

                    var q4 = f.InnerF.F.Size;
                    var q5 = f.InnerF.P.Size;
                    var q6 = f.InnerF.M().Size;
                    f.InnerF.E += (_) => {};

                    var q7 = f.InnerP.F.Size;
                    var q8 = f.InnerP.P.Size;
                    var q9 = f.InnerP.M().Size;
                    f.InnerP.E += (_) => {};
                }
            }
            class SimpleClass {
                public Car F;
                public Car P { get; set; }
                public Car M() { return new Car(); }
                public event Action<Car> E;

                public Inner InnerF;
                public Inner InnerP { get; set; }

                public class Inner {
                    public Car F;
                    public Car P { get; set; }
                    public Car M() { return new Car(); }
                    public event Action<Car> E;
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method()",
            (AsmName.Test, "SimpleClass/F"),
            (AsmName.Test, "SimpleClass/P"),
            (AsmName.Test, "SimpleClass/M()"),
            (AsmName.Test, "SimpleClass/E"),
            (AsmName.Test, "SimpleClass/InnerF"),
            (AsmName.Test, "SimpleClass/InnerP"),
            (AsmName.Test, "SimpleClass/Inner/F"),
            (AsmName.Test, "SimpleClass/Inner/P"),
            (AsmName.Test, "SimpleClass/Inner/M()"),
            (AsmName.Test, "SimpleClass/Inner/E"),
            (AsmName.TestProject, "TestProject/Entities/Car/Size")
        );
    }


    [Test]
    public void MemberAccessGeneric()
    {
        var graph = Build(@"
            using System;
            using TestProject.Entities;
            public class Test {
                private GenClass<sbyte> f1;
                private GenClass<byte>.Inner<short> f2;

                public void Method() {

                    f1.Method1();
                    f1.Method2<ushort>();
                    var g1 = f1.F;
                    var g2 = f1.P;

                    f2.Method1();
                    f2.Method2<int>();
                    var g3 = f2.F;
                    var g4 = f2.P;
                }
            }
            class GenClass<TX> {
                public TX F;
                public TX P { get; set; }
                public void Method1() {}
                public void Method2<TY>() {}

                public class Inner<TY> {
                    public TY F;
                    public TY P { get; set; }
                    public void Method1() {}
                    public void Method2<TZ>() {}
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/f1",
            (AsmName.Test, "GenClass<TX>"),
            (AsmName.CoreLib, "System/sbyte")
        );

        GraphAssert.HasLink(graph, "Test/f2",
            (AsmName.Test, "GenClass<TX>/Inner<TY>"),
            (AsmName.CoreLib, "System/byte"),
            (AsmName.CoreLib, "System/short")
        );

        GraphAssert.HasLink(graph, "Test/Method()",
            (AsmName.Test, "GenClass<TX>/F"),
            (AsmName.Test, "GenClass<TX>/P"),
            (AsmName.Test, "GenClass<TX>/Method1()"),
            (AsmName.Test, "GenClass<TX>/Method2<TY>()"),
            (AsmName.Test, "GenClass<TX>/Inner<TY>/F"),
            (AsmName.Test, "GenClass<TX>/Inner<TY>/P"),
            (AsmName.Test, "GenClass<TX>/Inner<TY>/Method1()"),
            (AsmName.Test, "GenClass<TX>/Inner<TY>/Method2<TZ>()"),
            (AsmName.CoreLib, "System/ushort"),
            (AsmName.CoreLib, "System/int")
        );
    }

    [Test]
    public void MemberAccessStatic()
    {
        var graph = Build(@"
            using System;
            using TestProject.Entities;
            public class Test {
                public void Method() {
                    var q1 = Utils.F.Size;
                    var q2 = Utils.P.Size;
                    var q3 = Utils.M().Size;
                    Utils.E += (_) => {};
                }
            }
            static class Utils {
                public static Car F = new Car();
                public static Car P { get; } = new Car();
                public static Car M() { return new Car(); }
                public static event Action<Car> E;
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method()",
            (AsmName.Test, "Utils"),
            (AsmName.Test, "Utils/F"),
            (AsmName.Test, "Utils/P"),
            (AsmName.Test, "Utils/M()"),
            (AsmName.Test, "Utils/E"),
            (AsmName.TestProject, "TestProject/Entities/Car/Size")
        );
    }


    [Test]
    public void MemberAccessStaticGeneric()
    {
        var graph = Build(@"
            using System;
            using TestProject;
            public class Test {
                public void Method() {
                    var q1 = Utils<int>.F.Val;
                    var q2 = Utils<uint>.P.Val;
                    var q3 = Utils<long>.M<byte>().Val;
                    Utils<double>.E += (_) => {};
                }
            }
            static class Utils<T> {
                public static GenericClass1<T> F = new GenericClass1<T>();
                public static GenericClass1<T> P { get; } = new GenericClass1<T>();
                public static GenericClass1<Y> M<Y>() { return new GenericClass1<Y>(); }
                public static event Action<T> E;
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method()",
            (AsmName.Test, "Utils<T>"),
            (AsmName.Test, "Utils<T>/F"),
            (AsmName.Test, "Utils<T>/P"),
            (AsmName.Test, "Utils<T>/M<Y>()"),
            (AsmName.Test, "Utils<T>/E"),
            (AsmName.TestProject, "TestProject/GenericClass1<T>/Val"),
            (AsmName.CoreLib, "System/int"),
            (AsmName.CoreLib, "System/uint"),
            (AsmName.CoreLib, "System/long"),
            (AsmName.CoreLib, "System/byte"),
            (AsmName.CoreLib, "System/double")
        );
    }

    [Test, Description("Var symbol must be ignored")]
    public void VarTypeIgnored()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test  {
                public void Method() {
                    var car = GetMethod();
                }
                public Car GetMethod() {
                    return new Car();
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method()",
            (AsmName.Test, "Test/GetMethod()")
        );

        GraphAssert.HasNotLink(graph, "Test/Method()",
            (AsmName.TestProject, "TestProject/Entities/Car")
        );
    }

    [Test, Description("Symbols for local variables and parameters must be ignored")]
    public void LocalAndParamsIgnored()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test {
                public void Method(Car car) {
                    Car car2 = new Car();
                    if (car == null) {
                        return;
                    }
                    if (car2 == null) {
                        return;
                    }
                }
            }
        ");

        var node = graph.GetNode(AsmName.Test, "Test/Method(Car)");

        Assert.That(node.Childs.Count(), Is.EqualTo(0));
        Assert.That(graph.GetOutgoingLinks(node).Count(), Is.EqualTo(3));
        GraphAssert.HasLink(graph, "Test/Method(Car)",
            (AsmName.TestProject, "TestProject/Entities/Car"),
            (AsmName.TestProject, "TestProject/Entities/Car/ctor()")
        );
    }

    [Test]
    public void ArrayParsed()
    {
        var graph = Build(@"
            using System.Collections.Generic;
            using TestProject.Entities;
            public class Test  {
                public void Method() {
                    var car = new Vehicle[] { new Car() };
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method()",
            (AsmName.TestProject, "TestProject/Entities/Vehicle"),
            (AsmName.TestProject, "TestProject/Entities/Car/ctor()")
        );
    }

    [Test]
    public void NullableCutOff()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test  {
                public void Method() {
                    int? i = null;
                    Car? car = null;
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method()",
            (AsmName.CoreLib, "System/int"),
            (AsmName.TestProject, "TestProject/Entities/Car")

        );
    }

    [Test]
    public void TupleDeconstruction()
    {
        var graph = Build(@"
            using System.Collections.Generic;
            using TestProject.Entities;
            public class Test  {
                public void Method() {
                    var (v1, t1) = GetMethod();
                    (Vehicle v2, string t2) = GetMethod();
                }
                public (Car car, string title) GetMethod() {
                    return (new Car(), ""1"");
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method()",
            (AsmName.Test, "Test/GetMethod()"),
            (AsmName.TestProject, "TestProject/Entities/Vehicle"),
            (AsmName.CoreLib, "System/string")
        );
    }

    [Test]
    public void DuplicateSymbolAdded()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test  {
                public void Method() {
                    var car1 = new Car();
                    var car2 = new Car();
                }
            }
        ");

        var node = graph.GetNode(AsmName.Test, "Test/Method()");
        var nodeCar = graph.GetNode(AsmName.TestProject, "TestProject/Entities/Car/ctor()");
        Assert.That(graph.GetOutgoingLinks(node).Count(l => l.Target.Id == nodeCar.Id), Is.EqualTo(2));
    }

    [Test]
    public void SelfMembers()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class BaseClass {
                public int F1;
                public int P1 { get; set; }

                public void BaseMethod() {}
            }
            public class Test : BaseClass {
                public int F2;
                public int P2 { get; set; }
                public void Method() {
                    F1 = 10;
                    P1 = 20;
                    F2 = 10;
                    P2 = 20;
                    this.BaseMethod();
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method()",
            (AsmName.Test, "BaseClass/F1"),
            (AsmName.Test, "BaseClass/P1"),
            (AsmName.Test, "BaseClass/BaseMethod()"),
            (AsmName.Test, "Test/F2"),
            (AsmName.Test, "Test/P2")
        );
    }

    [Test]
    public void QualifiedName()
    {
        var graph = Build(@"
            using TestProject;
            public class Test {
                public void Method() {
                    var s = TestProject.Statics.StrStatic1;
                    System.Threading.Thread currentThread = System.Threading.Thread.CurrentThread;
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method()",
            (AsmName.CoreLib, "System/Threading/Thread"),
            (AsmName.CoreLib, "System/Threading/Thread/CurrentThread"),
            (AsmName.TestProject, "TestProject/Statics/StrStatic1")
        );

        GraphAssert.HasNotLink(graph, "Test/Method()",
            (AsmName.TestProject, "TestProject"),
            (AsmName.CoreLib, "System"),
            (AsmName.CoreLib, "System/Threading")
        );
    }

    [Test]
    public void Tuple()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test {
                public void Method() {
                    var obj = (Car: new Car(), Size: new Size());
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method()",
            (AsmName.TestProject, "TestProject/Entities/Car/ctor()"),
            (AsmName.TestProject, "TestProject/Entities/Size")
        );
    }

    [Test]
    public void PrimitiveConstruct()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test  {
                public void Method() {
                    var x = new int();
                    var y = default(long);
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method()",
            (AsmName.CoreLib, "System/int"),
            (AsmName.CoreLib, "System/long")
        );
    }

    [Test]
    public void LocalFunctionSymbolIgnored()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test {
                public void TestMethod() {
                    void InnerMethod(Vehicle v) {}
                    InnerMethod(new Car());
                }
            }
        ");

        Assert.That(
            !graph.GetNode(AsmName.Test, "Test/TestMethod()")
                .Childs
                .Any(c => c.Id.EndsWith("InnerMethod(TestProject/Entities/Vehicle)"))
        );

        GraphAssert.HasLink(graph, "Test/TestMethod()",
            (AsmName.TestProject, "TestProject/Entities/Car/ctor()"),
            (AsmName.TestProject, "TestProject/Entities/Vehicle")
        );
    }

    [Test]
    public void CallExctensionGeneric()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test
            {
                public void TestMethod() {
                    var car = new Car();
                    car.Invoke1();
                    var obj = new object();
                    obj.Invoke2<Car>();
                }
            }

            public static class Extensions
            {
                public static void Invoke1<T>(this T obj) {}
                public static void Invoke2<T>(this object obj) {}
            }
        ");

        GraphAssert.HasLink(graph, "Test/TestMethod()",
            (AsmName.Test, "Extensions/Invoke1<T>(T)"),
            (AsmName.Test, "Extensions/Invoke2<T>(object)")
        );
    }

    [Test]
    public void IgnoreLinqRangeVariable()
    {
        var graph = Build(@"
            using System.Linq;
            public class Test
            {
                public void TestMethod() {
                    var numbers = new int[] { 5, 1, 4, 2, 3, 7 };
	                var v1 = from n in numbers
                        where n > 0
                        orderby n
                        select n;
                }
            }
        ");

        Assert.That(
            !graph.GetNode(AsmName.Test, "Test/TestMethod()")
                .Childs
                .Any()
        );
    }
}