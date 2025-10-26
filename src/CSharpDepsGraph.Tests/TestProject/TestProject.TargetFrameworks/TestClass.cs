﻿#pragma warning disable CS0219
#pragma warning disable IDE0059
namespace TestProject.TargetFrameworks;

public class TestClass
{
    public void TestMethod1()
    {
        var x = new int();
#if (NETSTANDARD2_1)
        var y = new long();
#else
        var z = new decimal();
#endif
    }

#if (NETSTANDARD2_1)
    public void TestMethod2()
    {
        var x = new long();
    }
#else
    public void TestMethod3()
    {
        var y = new decimal();
    }
#endif

#if (NETSTANDARD2_1)
    public void TestMethod4()
    {
        var x = new long();
    }
#else
    public void TestMethod4()
    {
        var y = new decimal();
    }
#endif
}
#pragma warning restore CS0219