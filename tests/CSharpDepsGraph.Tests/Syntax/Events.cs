using System.Linq;
using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Syntax;

public class EventsDeclaration : BaseSyntaxTests
{
    [Test]
    public void EventParsed()
    {
        var graph = Build(@"
            using System;
            public class Test {
                public event Action TestEvent;
                public void Method() {
                    TestEvent();
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/TestEvent",
            (AsmName.CoreLib, "System/Action")
        );

        GraphAssert.HasLink(graph, "Test/Method()",
            (AsmName.Test, "Test/TestEvent")
        );
    }

    [Test]
    public void AddRemoveParsed()
    {
        var graph = Build(@"
            using System;
            public class Test {
                private Action? _testEvent;

                public event Action TestEvent {
                    add {
                        _testEvent += value;
                    }
                    remove {
                        _testEvent -= value;
                    }
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/TestEvent",
            (AsmName.CoreLib, "System/Action"),
            (AsmName.Test, "Test/_testEvent")
        );

        var testClassNode = graph.GetNode("Test");
        Assert.That(testClassNode.Childs.SingleOrDefault(c => c.Uid.EndsWith(".add")), Is.Null);
        Assert.That(testClassNode.Childs.SingleOrDefault(c => c.Uid.EndsWith(".remove")), Is.Null);
    }

    [Test]
    public void ExplicitInterfaceParsed()
    {
        var graph = Build(@"
            using System;
            public interface ITest {
                public event Action TestEvent;
            }
            public interface ITest<T> {
                public event Action TestEvent;
            }
            public class Test : ITest, ITest<int> {
                event Action ITest.TestEvent {
                    add {}
                    remove {}
                }

                event Action ITest<int>.TestEvent {
                    add {}
                    remove {}
                }

                public event Action TestEvent {
                    add {}
                    remove {}
                }
            }
        ");

        GraphAssert.HasLink(graph, "Test/ITest.TestEvent",
            (AsmName.CoreLib, "System/Action"),
            (AsmName.Test, "ITest")
        );

        GraphAssert.HasLink(graph, "Test/ITest<int>.TestEvent",
            (AsmName.CoreLib, "System/Action"),
            (AsmName.CoreLib, "System/int"),
            (AsmName.Test, "ITest<T>")
        );

        GraphAssert.HasLink(graph, "Test/TestEvent",
            (AsmName.CoreLib, "System/Action")
        );
    }

    [Test]
    [Ignore("todo check in lang version")]
    public void PartialDefinition()
    {
        var graph = Build(@"
            using System;
            public partial class Test {
                public partial event Action TestEvent { get; set; }
            }
            public partial class Test {
                private Action _testEvent;
                public partial event Action TestEvent {
                    add { _testEvent += value; }
                    remove { _testEvent -= value; }
                    }
            }
        ");

        var node = graph.GetNode("Test/TestEvent");
        var nodeLocations = node.SyntaxLinks.ToArray();

        Assert.That(nodeLocations.Length, Is.EqualTo(2));
        Assert.That(nodeLocations[0].GetDisplayString(), Is.EqualTo($"{GraphFactory.TestFileName}:4:17"));
        Assert.That(nodeLocations[1].GetDisplayString(), Is.EqualTo($"{GraphFactory.TestFileName}:8:17"));
    }
}