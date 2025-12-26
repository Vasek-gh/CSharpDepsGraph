using CSharpDepsGraph.Tests.Syntax;
using NUnit.Framework;

namespace CSharpDepsGraph.Tests.Services;

public class FullyQualifiedUidGeneratorTests : BaseSyntaxTests
{
    [Test]
    public void Namespace()
    {
        var graph = Build(@"
            namespace Foo.Bar;
        ");

        Assert.That(graph.GetNode("Foo/Bar").Uid, Is.EqualTo("Test/Foo.Bar"));
    }

    [Test]
    public void TrivialTypes()
    {
        var graph = Build(@"
            namespace Bar {
                public class FooClass {}
                public interface FooInterface {}
                public struct FooStruct {}
                public record FooRecord {}
                public enum FooEnum {}
                public delegate void FooDelegate();
            }
        ");

        Assert.That(graph.GetNode("Bar/FooClass").Uid, Is.EqualTo("Test/Bar.FooClass"));
        Assert.That(graph.GetNode("Bar/FooInterface").Uid, Is.EqualTo("Test/Bar.FooInterface"));
        Assert.That(graph.GetNode("Bar/FooStruct").Uid, Is.EqualTo("Test/Bar.FooStruct"));
        Assert.That(graph.GetNode("Bar/FooRecord").Uid, Is.EqualTo("Test/Bar.FooRecord"));
        Assert.That(graph.GetNode("Bar/FooEnum").Uid, Is.EqualTo("Test/Bar.FooEnum"));
        Assert.That(graph.GetNode("Bar/FooDelegate").Uid, Is.EqualTo("Test/Bar.FooDelegate"));
    }

    [Test]
    public void GenericTypes()
    {
        var graph = Build(@"
            namespace Bar {
                public class FooClass<T> {}
                public interface FooInterface<T> {}
                public struct FooStruct<T> {}
                public record FooRecord<T> {}
                public delegate void FooDelegate<T>();
            }
        ");

        Assert.That(graph.GetNode("Bar/FooClass<T>").Uid, Is.EqualTo("Test/Bar.FooClass<T>"));
        Assert.That(graph.GetNode("Bar/FooInterface<T>").Uid, Is.EqualTo("Test/Bar.FooInterface<T>"));
        Assert.That(graph.GetNode("Bar/FooStruct<T>").Uid, Is.EqualTo("Test/Bar.FooStruct<T>"));
        Assert.That(graph.GetNode("Bar/FooRecord<T>").Uid, Is.EqualTo("Test/Bar.FooRecord<T>"));
        Assert.That(graph.GetNode("Bar/FooDelegate<T>").Uid, Is.EqualTo("Test/Bar.FooDelegate<T>"));
    }

    [Test]
    public void MethodTrivial()
    {
        var graph = Build(@"
            class Foo{
                void Bar() {}
            }
        ");

        Assert.That(graph.GetNode("Foo/Bar()").Uid, Is.EqualTo("Test/Foo.Bar()"));
    }

    [Test]
    public void MethodPrimitiveArgument()
    {
        var graph = Build(@"
            class Foo{
                void Bar(int a) {}
            }
        ");

        Assert.That(graph.GetNode("Foo/Bar(int)").Uid, Is.EqualTo("Test/Foo.Bar(int)"));
    }

    [Test]
    public void MethodInternalArgument()
    {
        var graph = Build(@"
            using System;
            namespace FooSpace {
                class BarClass{}
            }
            class Foo{
                void Bar(FooSpace.BarClass a) {}
            }

        ");

        Assert.That(graph.GetNode("Foo/Bar(BarClass)").Uid, Is.EqualTo("Test/Foo.Bar(FooSpace.BarClass)"));
    }

    [Test]
    public void MethodExternalArgument()
    {
        var graph = Build(@"
            using System;
            class Foo{
                void Bar(TimeSpan a) {}
            }
        ");

        Assert.That(graph.GetNode("Foo/Bar(TimeSpan)").Uid, Is.EqualTo("Test/Foo.Bar(System.TimeSpan)"));
    }

    [Test]
    public void MethodGenericArgument()
    {
        var graph = Build(@"
            using System;
            class Foo{
                void Bar<T>(T a) {}
            }
        ");

        Assert.That(graph.GetNode("Foo/Bar<T>(T)").Uid, Is.EqualTo("Test/Foo.Bar<T>(T)"));
    }

    [Test]
    public void MethodPointerArgument()
    {
        var graph = Build(@"
            using System;
            unsafe class Foo{
                void Bar(int* a) {}
            }
        ");

        Assert.That(graph.GetNode("Foo/Bar(int*)").Uid, Is.EqualTo("Test/Foo.Bar(int*)"));
    }

    [Test]
    public void MethodArrayArgument()
    {
        var graph = Build(@"
            using System;
            unsafe class Foo{
                void Bar(int[] a) {}
            }
        ");

        Assert.That(graph.GetNode("Foo/Bar(int[])").Uid, Is.EqualTo("Test/Foo.Bar(int[])"));
    }

    [Test]
    public void MethodNullableArgument()
    {
        var graph = Build(@"
            using System;
            unsafe class Foo{
                void Bar(int? a) {}
                void Bar(Foo? a) {}
            }
        ");

        Assert.That(graph.GetNode("Foo/Bar(int?)").Uid, Is.EqualTo("Test/Foo.Bar(int?)"));
        Assert.That(graph.GetNode("Foo/Bar(Foo?)").Uid, Is.EqualTo("Test/Foo.Bar(Foo?)"));
    }

    [Test]
    public void MethodParamsArgument()
    {
        var graph = Build(@"
            using System;
            unsafe class Foo{
                void Bar(params int[] a) {}
            }
        ");

        Assert.That(graph.GetNode("Foo/Bar(params int[])").Uid, Is.EqualTo("Test/Foo.Bar(int[])"));
    }

    [Test]
    public void Indexer()
    {
        var graph = Build(@"
            using System;
            unsafe class Foo{
                public int this[int key] => 0;
            }
        ");

        Assert.That(graph.GetNode("Foo/this[int]").Uid, Is.EqualTo("Test/Foo.this[int]"));
    }
}