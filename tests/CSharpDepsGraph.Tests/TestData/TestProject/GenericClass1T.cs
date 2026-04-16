namespace TestProject;

public class GenericClass1<T>
{
    public T Val { get; set; }

    public GenericClass1()
    {
        Val = default!;
    }

    public GenericClass1(T value)
    {
        Val = value;
    }

    public void Show()
    {
    }

    public void Show<Y>()
    {
    }
}