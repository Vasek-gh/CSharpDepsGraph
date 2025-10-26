using TestProject.Generated;

internal class Dbg
{
    public void TestGenerators()
    {
        new GeneratedClass().GeneratedMethod();
        new GeneratedClassPartial().PartialMethod();
        new GeneratedClassPartial().GeneratedMethod();
    }
}