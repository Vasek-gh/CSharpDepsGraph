namespace TestProject.Entities;

public class Car : Vehicle
{
    public Size Size { get; set; } = null!;
    public CarTransmission Transmission { get; set; }
    public IEnumerable<CarDoor> Doors { get; set; } = Array.Empty<CarDoor>();

    public Car()
    {
        Size = null!;
        Transmission = null!;
    }

    public Car(Size size)
    {
        Size = size;
        Transmission = null!;
    }

    public Car(Size size, CarTransmission transmission)
    {
        Size = size;
        Transmission = transmission;
    }
}
