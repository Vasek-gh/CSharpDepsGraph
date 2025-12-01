using Microsoft.CodeAnalysis;

namespace TestProject.Generators;

[Generator]
public class TestGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyNameProvider = context.CompilationProvider.Select((s, _)  => s.AssemblyName);

        var generatedClassText = @"
                namespace TestProject.Generated;
                public class GeneratedClass
                {
                    public void GeneratedMethod()
                    {

                    }
                }
            ";

        var generatedClassPartialText = @"
                namespace TestProject.Generated;
                public partial class GeneratedClassPartial
                {
                    public partial void PartialMethod()
                    {

                    }

                    public void GeneratedMethod()
                    {

                    }
                }
            ";

        var generatedFooText = @"
                namespace TestProject.Generated;
                public class Foo
                {
                    public void PublicMethod()
                    {
                        PrivateMethod();
                    }

                    private void PrivateMethod()
                    {
                        var a = 0;
                    }
                }
            ";

        context.RegisterSourceOutput(assemblyNameProvider, (spc, assemblyName) =>
        {
            if (assemblyName.EndsWith("Cli"))
            {
                spc.AddSource("GeneratedFoo.cs", generatedFooText);
            }
            else
            {
                spc.AddSource("GeneratedClass.cs", generatedClassText);
                spc.AddSource("GeneratedClassPartial.cs", generatedClassPartialText);
            }
        });
    }
}