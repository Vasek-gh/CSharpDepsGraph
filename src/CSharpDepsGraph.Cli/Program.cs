using CSharpDepsGraph.Cli.CommandLine;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace CSharpDepsGraph.Cli;

internal class Program
{
    public static async Task<int> Main(string[] args)
    {
        var result = await Run(args, new CommandFactory());

        Console.WriteLine(GC.GetTotalMemory(false).ToString("N0")); // todo kill
        Console.WriteLine("Wait"); // todo kill
        Console.ReadKey();

        return result;
    }

    public static async Task<int> Run(string[] args, ICommandFactory commandFactory)
    {
        var rootCommand = new RootCommand($"{nameof(CSharpDepsGraph)} cli tool");
        rootCommand.AddCommand(new JsonExportCliCommand(commandFactory));
        rootCommand.AddCommand(new DgmlExportCliCommand(commandFactory));
        rootCommand.AddCommand(new GraphvizExportCliCommand(commandFactory));

        var result = await new CommandLineBuilder(rootCommand)
            .UseDefaults()
            .UseExceptionHandler(HandleException)
            .UseParseErrorReporting()
            .Build()
            .InvokeAsync(args);

        return result;
    }

    private static void HandleException(Exception ex, InvocationContext ctx)
    {
        ctx.ExitCode = 1;
        if (ex is OperationCanceledException)
        {
            return;
        }

        var error = ex is CSharpDepsGraphException
            ? ex.Message
            : ex.ToString();

        SetForegroundColor(ConsoleColor.Red);
        try
        {
            ctx.Console.Error.Write(error);
        }
        finally
        {
            SetForegroundColor(null);
        }
    }

    private static void SetForegroundColor(ConsoleColor? color)
    {
        try
        {
            if (Console.IsOutputRedirected)
            {
                return;
            }

            if (color == null)
            {
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = color.Value;
            }
        }
        catch (PlatformNotSupportedException)
        {
            return;
        }
    }
}