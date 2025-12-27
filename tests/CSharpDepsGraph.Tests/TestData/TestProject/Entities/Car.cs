namespace TestProject.Entities;

public class Car : Vehicle
{
    public Size Size { get; set; }

    public Car()
    {
        Size = null!;
    }

    public Car(Size size)
    {
        Size = size;
    }
}
