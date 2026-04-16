using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.CommandLine;
using TestProject.Cli;
using TestProject.Entities;
using TestProject.Generated;

ILogger logger = NullLogger.Instance;
RootCommand rootCommand = new RootCommand();

var car = new Car();
new Foo().PublicMethod();
new GenComment().PublicMethod();
new GeneratedClass().GeneratedMethod();
new GeneratedClassPartial().PartialMethod();
new GeneratedClassPartial().GeneratedMethod();

Console.WriteLine("Hello, World!");
