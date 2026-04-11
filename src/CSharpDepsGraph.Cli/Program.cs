using CSharpDepsGraph.Cli.CommandLine;
using System.CommandLine;

namespace CSharpDepsGraph.Cli;

internal class Program
{
    public static Task<int> Main(string[] args)
    {
        return Run(args, new CommandFactory());
    }

    public static async Task<int> Run(string[] args, ICommandFactory commandFactory)
    {
        var rootCommand = new RootCommand($"{nameof(CSharpDepsGraph)} cli tool");
        rootCommand.Subcommands.Add(new JsonExportCliCommand(commandFactory));
        rootCommand.Subcommands.Add(new DgmlExportCliCommand(commandFactory));
        rootCommand.Subcommands.Add(new GraphvizExportCliCommand(commandFactory));

        var parseResult = rootCommand.Parse(args);

        try
        {
            return await parseResult.InvokeAsync(new InvocationConfiguration()
            {
                EnableDefaultExceptionHandler = false,
            });
        }
        catch (Exception e)
        {
            HandleException(e);
            return 1;
        }
    }

    private static void HandleException(Exception ex)
    {
        if (ex is OperationCanceledException)
        {
            return;
        }

        var error = ex is CSharpDepsGraphException
            ? ex.Message
            : "Unhandled exception: " + ex.ToString();

        SetForegroundColor(ConsoleColor.Red);
        try
        {
            Console.Error.WriteLine(error);
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