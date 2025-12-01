using TestProject.Cli;
using TestProject.Entities;
using TestProject.Generated;

var car = new Car();
new Foo().PublicMethod();
new GenComment().PublicMethod();
new GeneratedClass().GeneratedMethod();
new GeneratedClassPartial().PartialMethod();
new GeneratedClassPartial().GeneratedMethod();

Console.WriteLine("Hello, World!");
