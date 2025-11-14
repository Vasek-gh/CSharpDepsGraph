namespace TestProject;

public class GenericClass2<T>
{
    public T Val { get; set; } = default!;

    public void Show()
    {
    }

    public void Show<Y>()
    {
    }
}
