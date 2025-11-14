namespace TestProject.Entities;

public class Vehicle
{
    public bool Running { get; set; }
    public double Price { get; set; }

    public void TurnOn()
    {
        Running = true;
    }

#pragma warning disable IDE0060
    public void TurnOff(string reason)
    {
        Running = false;
    }
#pragma warning restore IDE0060
}