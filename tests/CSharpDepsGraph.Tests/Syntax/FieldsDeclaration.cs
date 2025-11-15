using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Syntax;

public class FieldsDeclaration : BaseSyntaxTests
{
    [Test]
    public void TypeParsed()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test {
                private Car _field1;
            }
        ");

        GraphAssert.HasLink(graph, "Test/_field1",
            (AsmName.TestProject, "TestProject/Entities/Car")
        );
    }

    [Test]
    public void InitializerParsed()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test {
                private Vehicle _field1 = new Car();
            }
        ");

        GraphAssert.HasLink(graph, "Test/_field1",
            (AsmName.TestProject, "TestProject/Entities/Vehicle"),
            (AsmName.TestProject, "TestProject/Entities/Car/ctor()")
        );
    }

    [Test]
    public void MiltipleFieldsInOneDeclaration()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test {
                private Vehicle _field1 = new Car(), _field2 = new Airplane();
            }
        ");

        GraphAssert.HasLink(graph, "Test/_field1",
            (AsmName.TestProject, "TestProject/Entities/Vehicle"),
            (AsmName.TestProject, "TestProject/Entities/Car/ctor()")
        );

        GraphAssert.HasLink(graph, "Test/_field2",
            (AsmName.TestProject, "TestProject/Entities/Vehicle"),
            (AsmName.TestProject, "TestProject/Entities/Airplane")
        );
    }

    [Test]
    public void BackingFieldIgnored()
    {
        var graph = Build(@"
            public class Test  {
                string Prop1 { get; }
            }
        ");

        GraphAssert.HasSymbol(graph, "Test/Prop1");
        GraphAssert.HasNotSymbol(graph, (AsmName.Test, "Test.<Prop1>k__BackingField"));
    }
}