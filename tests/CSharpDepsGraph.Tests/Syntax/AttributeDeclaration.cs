using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Syntax;

public class AttributeDeclaration : BaseSyntaxTests
{
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

        GraphAssert.HasLink(graph, "Test.Prop",
            (AsmName.TestProject, "TestProject.Attributes.SimpleAttribute.ctor(int,string)"),
            (AsmName.TestProject, "TestProject.Constants.IntConst1"),
            (AsmName.TestProject, "TestProject.Constants.StrConst1"),
            (AsmName.TestProject, "TestProject.Constants.StrConst2")
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

        GraphAssert.HasLink(graph, "Test._field",
            (AsmName.TestProject, "TestProject.Attributes.SimpleAttribute.ctor(int,string)"),
            (AsmName.TestProject, "TestProject.Constants.IntConst1"),
            (AsmName.TestProject, "TestProject.Constants.StrConst1"),
            (AsmName.TestProject, "TestProject.Constants.StrConst2")
        );
    }

    [Test]
    public void ClassAttribute()
    {
        var graph = Build(@"
            using TestProject;
            using TestProject.Attributes;
            [Simple(Constants.IntConst1, Constants.StrConst1 + Constants.StrConst2)]
            public class Test {}
        ");

        GraphAssert.HasLink(graph, "Test",
            (AsmName.TestProject, "TestProject.Attributes.SimpleAttribute.ctor(int,string)"),
            (AsmName.TestProject, "TestProject.Constants.IntConst1"),
            (AsmName.TestProject, "TestProject.Constants.StrConst1"),
            (AsmName.TestProject, "TestProject.Constants.StrConst2")
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

        GraphAssert.HasLink(graph, "Test.Method()",
            (AsmName.TestProject, "TestProject.Attributes.SimpleAttribute.ctor(int,string)"),
            (AsmName.TestProject, "TestProject.Constants.IntConst1"),
            (AsmName.TestProject, "TestProject.Constants.StrConst1"),
            (AsmName.TestProject, "TestProject.Constants.StrConst2")
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

        GraphAssert.HasLink(graph, "Test.Method(int)",
            (AsmName.TestProject, "TestProject.Attributes.SimpleAttribute.ctor(int,string)"),
            (AsmName.TestProject, "TestProject.Constants.IntConst1"),
            (AsmName.TestProject, "TestProject.Constants.StrConst1"),
            (AsmName.TestProject, "TestProject.Constants.StrConst2")
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

        GraphAssert.HasLink(graph, "Test.Method()",
            (AsmName.TestProject, "TestProject.Attributes.SimpleAttribute.ctor(int,string)"),
            (AsmName.TestProject, "TestProject.Constants.IntConst1"),
            (AsmName.TestProject, "TestProject.Constants.StrConst1"),
            (AsmName.TestProject, "TestProject.Constants.StrConst2")
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

        GraphAssert.HasLink(graph, "Test.TestMethod()",
            (AsmName.TestProject, "TestProject.Attributes.SimpleAttribute.ctor(int,string)"),
            (AsmName.TestProject, "TestProject.Constants.IntConst1")
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

        GraphAssert.HasLink(graph, "Test.TestMethod()",
            (AsmName.TestProject, "TestProject.Attributes.SimpleAttribute.ctor(int,string)"),
            (AsmName.TestProject, "TestProject.Constants.IntConst1")
        );
    }
}