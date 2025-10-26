using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace TestProject.Generators
{
    [Generator]
    public class TestGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var generatedClassText = """
                namespace TestProject.Generated;
                public class GeneratedClass
                {
                    public void GeneratedMethod()
                    {

                    }
                }
            """;

            var generatedClassPartialText = """
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
            """;

            context.RegisterPostInitializationOutput(i =>
            {
                i.AddSource("GeneratedClass.g.cs", generatedClassText);
                i.AddSource("GeneratedClassPartial.g.cs", generatedClassPartialText);
            });
        }
    }
}