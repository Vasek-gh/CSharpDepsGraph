namespace TestProject;

public class Preprocessor1
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