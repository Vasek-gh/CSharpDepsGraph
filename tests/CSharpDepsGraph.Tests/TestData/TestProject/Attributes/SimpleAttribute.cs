namespace TestProject.Attributes;

public class SimpleAttribute : Attribute
{
    public int AttrProp { get; set; }
#pragma warning disable IDE0060
    public SimpleAttribute(int intArg, string strArg)
    {
    }
#pragma warning restore IDE0060
}
