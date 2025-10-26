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
        var rootCommand = new RootCommand($"{nameof(CSharpDepsGraph)} cli tool");
        rootCommand.AddCommand(new JsonExportCliCommand());
        rootCommand.AddCommand(new DgmlExportCliCommand());
        rootCommand.AddCommand(new GraphvizExportCliCommand());

        return await new CommandLineBuilder(rootCommand)
            .UseDefaults()
            .UseExceptionHandler(HandleException)
            .Build()
            .InvokeAsync(args);
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