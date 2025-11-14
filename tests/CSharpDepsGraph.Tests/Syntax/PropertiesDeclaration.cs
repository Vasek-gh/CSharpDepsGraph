using System.Linq;
using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Syntax;

public class PropertiesDeclaration : BaseSyntaxTests
{
    [Test]
    public void TypeParsed()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test {
                private Car Prop1 { get; set; }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Prop1",
            (AsmName.TestProject, "TestProject/Entities/Car")
        );
    }

    [Test]
    public void Initializer()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test {
                private Vehicle Prop1 { get; } = new Car();
            }
        ");

        GraphAssert.HasLink(graph, "Test/Prop1",
            (AsmName.TestProject, "TestProject/Entities/Vehicle"),
            (AsmName.TestProject, "TestProject/Entities/Car/Car()")
        );
    }

    [Test]
    public void GetSet()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test {
                private Vehicle Prop1
                {
                    get { return new Car(); }
                    set { value = new Airplane(); }
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Prop1",
            (AsmName.TestProject, "TestProject/Entities/Vehicle"),
            (AsmName.TestProject, "TestProject/Entities/Car/Car()"),
            (AsmName.TestProject, "TestProject/Entities/Airplane")
        );
    }

    [Test]
    public void ExpressionBodied()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test {
                private Vehicle Prop1 => new Car();
            }
        ");

        GraphAssert.HasLink(graph, "Test/Prop1",
            (AsmName.TestProject, "TestProject/Entities/Car/Car()")
        );
    }

    [Test]
    public void GetSetExpressionBodied()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public class Test {
                private Vehicle Prop1
                {
                    get => new Car();
                    set => value = new Airplane();
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/Prop1",
            (AsmName.TestProject, "TestProject/Entities/Vehicle"),
            (AsmName.TestProject, "TestProject/Entities/Car/Car()"),
            (AsmName.TestProject, "TestProject/Entities/Airplane")
        );
    }

    [Test]
    public void ExplicitInterfaceParsed()
    {
        var graph = Build(@"
            public interface ITest {
                string Prop1 { get; }
            }
            public interface ITest<T> {
                string Prop1 { get; }
            }
            public class Test : ITest, ITest<int>  {
                string ITest.Prop1 { get; }
                string ITest<int>.Prop1 { get; }
                public string Prop1 { get; }
            }
        ");

        GraphAssert.HasLink(graph, "Test/ITest.Prop1",
            (AsmName.CoreLib, "System/string"),
            (AsmName.Test, "ITest")
        );

        GraphAssert.HasLink(graph, "Test/ITest<int>.Prop1",
            (AsmName.CoreLib, "System/string"),
            (AsmName.CoreLib, "System/int"),
            (AsmName.Test, "ITest<T>")
        );

        GraphAssert.HasLink(graph, "Test/Prop1",
            (AsmName.CoreLib, "System/string")
        );
    }

    [Test]
    public void GetSetIgnored()
    {
        var graph = Build(@"
            public class Test  {
                string Prop1 { get; set; }
            }
        ");

        GraphAssert.HasSymbol(graph, "Test/Prop1");
        GraphAssert.HasNotSymbol(graph,
            (AsmName.Test, "Test/Prop1/get"),
            (AsmName.Test, "Test/Prop1/set")
        );
    }

    [Test]
    public void RecordProps()
    {
        var graph = Build(@"
            using TestProject.Entities;
            public record Test(Car Car, Size Size)
            {
                public Car Car { get; init; } = Car;
                public Airplane Airplane { get; set; }
            }
        ");

        GraphAssert.HasSymbol(graph, "Test");
        GraphAssert.HasSymbol(graph, "Test/Car");
        GraphAssert.HasSymbol(graph, "Test/Airplane");
        GraphAssert.HasSymbol(graph, "Test/Size");

        Assert.That(
            graph.GetNode(AsmName.Test, "Test").Childs
                .GroupBy(c => c.Id)
                .Count(g => g.Count() > 1),
            Is.EqualTo(0)
            );
    }
}