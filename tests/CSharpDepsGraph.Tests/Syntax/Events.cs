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
        Assert.That(testClassNode.Childs.SingleOrDefault(c => c.Id.EndsWith(".add")), Is.Null);
        Assert.That(testClassNode.Childs.SingleOrDefault(c => c.Id.EndsWith(".remove")), Is.Null);
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
}