using TestProject.Entities;

namespace TestProject;

#if (NETSTANDARD2_1)
public class Preprocessor2<T>
#elif (NET6_0)
public class Preprocessor2<T> where T : Car
#elif (NET8_0)
public class Preprocessor2<T> where T : Airplane
#endif
{
    public void Foo()
    {

    }
}