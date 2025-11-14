namespace TestProject;

#if (NETSTANDARD2_1)
public class Preprocessor3_1
#else
public class Preprocessor3_2
#endif
{
    public void Foo()
    {

    }
}

public class Preprocessor3User
{
    public void Test()
    {
        var preprocessor3 = new
#if (NETSTANDARD2_1)
        Preprocessor3_1();
#else
        Preprocessor3_2();
#endif
        preprocessor3.Foo();
    }
}