using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Syntax;

public class AttributeDeclaration : BaseSyntaxTests
{
    [Test]
    public void AssemblyAttribute()
    {
        var graph = Build(@"
            using System.Runtime.CompilerServices;
            [assembly: InternalsVisibleTo(""Foo.Bar"")]
        ");

        GraphAssert.HasLink(graph, "",
            (AsmName.CoreLib, "System/Runtime/CompilerServices/InternalsVisibleToAttribute/ctor(string)")
        );

        GraphAssert.HasNotLink(graph, "",
            (AsmName.CoreLib, "System/Runtime/CompilerServices/InternalsVisibleToAttribute")
        );
    }

    [Test]
    public void ClassAttribute()
    {
        var graph = Build(@"
            using TestProject;
            using TestProject.Attributes;
            [Simple(Constants.IntConst1, Constants.StrConst1 + Constants.StrConst2, AttrProp = 1)]
            public class Test {}
        ");

        GraphAssert.HasLink(graph, "Test",
            (AsmName.TestProject, "TestProject/Attributes/SimpleAttribute/ctor(int, string)"),
            (AsmName.TestProject, "TestProject/Attributes/SimpleAttribute/AttrProp"),
            (AsmName.TestProject, "TestProject/Constants/IntConst1"),
            (AsmName.TestProject, "TestProject/Constants/StrConst1"),
            (AsmName.TestProject, "TestProject/Constants/StrConst2")
        );

        GraphAssert.HasNotLink(graph, "Test",
            (AsmName.TestProject, "TestProject/Attributes/SimpleAttribute")
        );
    }

    [Test]
    public void FieldAttribute()
    {
        var graph = Build(@"
            using TestProject;
            using TestProject.Attributes;
            public class Test {
                [Simple(Constants.IntConst1, Constants.StrConst1 + Constants.StrConst2)]
                private int _field;
            }
        ");

        GraphAssert.HasLink(graph, "Test/_field",
            (AsmName.TestProject, "TestProject/Attributes/SimpleAttribute/ctor(int, string)"),
            (AsmName.TestProject, "TestProject/Constants/IntConst1"),
            (AsmName.TestProject, "TestProject/Constants/StrConst1"),
            (AsmName.TestProject, "TestProject/Constants/StrConst2")
        );

        GraphAssert.HasNotLink(graph, "Test/_field",
            (AsmName.TestProject, "TestProject/Attributes/SimpleAttribute")
        );
    }

    [Test]
    public void PropertyAttribute()
    {
        var graph = Build(@"
            using TestProject;
            using TestProject.Attributes;
            public class Test {
                [Simple(Constants.IntConst1, Constants.StrConst1 + Constants.StrConst2)]
                public int Prop { get; }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Prop",
            (AsmName.TestProject, "TestProject/Attributes/SimpleAttribute/ctor(int, string)"),
            (AsmName.TestProject, "TestProject/Constants/IntConst1"),
            (AsmName.TestProject, "TestProject/Constants/StrConst1"),
            (AsmName.TestProject, "TestProject/Constants/StrConst2")
        );

        GraphAssert.HasNotLink(graph, "Test/Prop",
            (AsmName.TestProject, "TestProject/Attributes/SimpleAttribute")
        );
    }

    [Test]
    public void MethodAttribute()
    {
        var graph = Build(@"
            using TestProject;
            using TestProject.Attributes;
            public class Test {
                [Simple(Constants.IntConst1, Constants.StrConst1 + Constants.StrConst2)]
                public void Method() {}
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method()",
            (AsmName.TestProject, "TestProject/Attributes/SimpleAttribute/ctor(int, string)"),
            (AsmName.TestProject, "TestProject/Constants/IntConst1"),
            (AsmName.TestProject, "TestProject/Constants/StrConst1"),
            (AsmName.TestProject, "TestProject/Constants/StrConst2")
        );

        GraphAssert.HasNotLink(graph, "Test/Method()",
            (AsmName.TestProject, "TestProject/Attributes/SimpleAttribute")
        );
    }

    [Test]
    public void MethodArgumentAttribute()
    {
        var graph = Build(@"
            using TestProject;
            using TestProject.Attributes;
            public class Test {
                public void Method([Simple(Constants.IntConst1, Constants.StrConst1 + Constants.StrConst2)] int arg) {}
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method(int)",
            (AsmName.TestProject, "TestProject/Attributes/SimpleAttribute/ctor(int, string)"),
            (AsmName.TestProject, "TestProject/Constants/IntConst1"),
            (AsmName.TestProject, "TestProject/Constants/StrConst1"),
            (AsmName.TestProject, "TestProject/Constants/StrConst2")
        );

        GraphAssert.HasNotLink(graph, "Test/Method(int)",
            (AsmName.TestProject, "TestProject/Attributes/SimpleAttribute")
        );
    }

    [Test]
    public void MethodReturnAttribute()
    {
        var graph = Build(@"
            using TestProject;
            using TestProject.Attributes;
            public class Test {
                [return: Simple(Constants.IntConst1, Constants.StrConst1 + Constants.StrConst2)]
                public int Method() { return 0; }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Method()",
            (AsmName.TestProject, "TestProject/Attributes/SimpleAttribute/ctor(int, string)"),
            (AsmName.TestProject, "TestProject/Constants/IntConst1"),
            (AsmName.TestProject, "TestProject/Constants/StrConst1"),
            (AsmName.TestProject, "TestProject/Constants/StrConst2")
        );

        GraphAssert.HasNotLink(graph, "Test/Method()",
            (AsmName.TestProject, "TestProject/Attributes/SimpleAttribute")
        );
    }

    [Test]
    public void LocalFunctionArgumentAttribute()
    {
        var graph = Build(@"
            using TestProject;
            using TestProject.Attributes;
            public class Test {
                public void TestMethod() {
                    void InnerMethod([Simple(Constants.IntConst1, ""1"")] int arg) {}
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/TestMethod()",
            (AsmName.TestProject, "TestProject/Attributes/SimpleAttribute/ctor(int, string)"),
            (AsmName.TestProject, "TestProject/Constants/IntConst1")
        );

        GraphAssert.HasNotLink(graph, "Test/TestMethod()",
            (AsmName.TestProject, "TestProject/Attributes/SimpleAttribute")
        );
    }

    [Test]
    public void LocalFunctionReturnAttribute()
    {
        var graph = Build(@"
            using TestProject;
            using TestProject.Attributes;
            public class Test {
                public void TestMethod() {
                    [return: Simple(Constants.IntConst1, ""1"")]
                    int InnerMethod() { return 0; }
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/TestMethod()",
            (AsmName.TestProject, "TestProject/Attributes/SimpleAttribute/ctor(int, string)"),
            (AsmName.TestProject, "TestProject/Constants/IntConst1")
        );

        GraphAssert.HasNotLink(graph, "Test/TestMethod()",
            (AsmName.TestProject, "TestProject/Attributes/SimpleAttribute")
        );
    }

    [Test]
    public void ImplicitlyAttributeConstructor()
    {
        var graph = Build(@"
            using System;
            public class BarAttribute : Attribute {}
            public class Test {
                [Bar]
                public void TestMethod() {}
            }
        ");

        GraphAssert.HasLink(graph, "Test/TestMethod()",
            (AsmName.Test, "BarAttribute")
        );
    }

    [Test]
    public void GenericImplicitlyAttributeConstructor()
    {
        var graph = Build(@"
            using System;
            public class BarAttribute<T> : Attribute {}

            public class Test {
                [Bar<int>]
                public void TestMethod() {}
            }
        ");

        GraphAssert.HasExactLink(graph, "Test/TestMethod()",
            (AsmName.Test, "BarAttribute<T>"),
            (AsmName.CoreLib, "System/int")
        );
    }

    [Test]
    public void QualifiedImplicitlyAttributeConstructor()
    {
        var graph = Build(@"
            using System;
            namespace Foo {
                public class BarAttribute : Attribute {}

                public class BarClass {
                    public class BarAttribute : Attribute {}
                }

                public class BarClass<T> {
                    public class BarAttribute : Attribute {}
                }
            }
            public class Test {
                [Foo.Bar]
                public void TestMethod1() {}

                [Foo.BarClass.Bar]
                public void TestMethod2() {}

                [Foo.BarClass<byte>.Bar]
                public void TestMethod3() {}
            }
        ");

        GraphAssert.HasExactLink(graph, "Test/TestMethod1()",
            (AsmName.Test, "Foo/BarAttribute")
        );

        GraphAssert.HasExactLink(graph, "Test/TestMethod2()",
            (AsmName.Test, "Foo/BarClass"),
            (AsmName.Test, "Foo/BarClass/BarAttribute")
        );

        GraphAssert.HasExactLink(graph, "Test/TestMethod3()",
            (AsmName.Test, "Foo/BarClass<T>"),
            (AsmName.Test, "Foo/BarClass<T>/BarAttribute"),
            (AsmName.CoreLib, "System/byte")
        );
    }

    [Test]
    public void GenericQualifiedImplicitlyAttributeConstructor()
    {
        var graph = Build(@"
            using System;
            namespace Foo {
                public class BarAttribute<T> : Attribute {}

                public class BarClass {
                    public class BarAttribute<T> : Attribute {}
                }

                public class BarClass<TX> {
                    public class BarAttribute<TY> : Attribute {}
                }
            }
            public class Test {
                [Foo.Bar<int>]
                public void TestMethod1() {}

                [Foo.BarClass.Bar<int>]
                public void TestMethod2() {}

                [Foo.BarClass<byte>.Bar<int>]
                public void TestMethod3() {}
            }
        ");

        GraphAssert.HasExactLink(graph, "Test/TestMethod1()",
            (AsmName.Test, "Foo/BarAttribute<T>"),
            (AsmName.CoreLib, "System/int")
        );

        GraphAssert.HasExactLink(graph, "Test/TestMethod2()",
            (AsmName.Test, "Foo/BarClass"),
            (AsmName.Test, "Foo/BarClass/BarAttribute<T>"),
            (AsmName.CoreLib, "System/int")
        );

        GraphAssert.HasExactLink(graph, "Test/TestMethod3()",
            (AsmName.Test, "Foo/BarClass<TX>"),
            (AsmName.Test, "Foo/BarClass<TX>/BarAttribute<TY>"),
            (AsmName.CoreLib, "System/int"),
            (AsmName.CoreLib, "System/byte")
        );
    }
}