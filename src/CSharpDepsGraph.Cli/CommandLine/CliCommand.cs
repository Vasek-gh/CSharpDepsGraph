using CSharpDepsGraph.Building;
using CSharpDepsGraph.Cli.Options;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace CSharpDepsGraph.Cli.CommandLine;

internal interface IBuildCommand
{
    Task Execute(ILoggerFactory loggerFactory, InvocationContext ctx);
}

internal interface IBuildCommandFactory
{
    IBuildCommand Create(CliCommand command);
}

class BaseBuildCommandFactory
{
    IBuildCommand Create(CliCommand command)
    {

    }
}

internal class BaseComand
{
    private readonly Command _command;
    private readonly OptionsHost<BuildOptions> _buildOptionsHost;
    private readonly OptionsHost<GraphBuildOptions> _graphBuildOptionsHost;

    public BaseComand()
    {
        _command = new Command("", "");
    }
}

internal class Command1 : BaseComand
{
    private readonly OptionsHost<JsonExportOptions> _optionsHost;

    public Command1() : base()
    {

    }
}

internal sealed class CliCommand : Command
{
    private readonly IBuildCommand _buildCommand;

    public CliCommand(string name, string description, IBuildCommandFactory buildCommandFactory)
        : base(name, description)
    {
        this.SetHandler(Execute);

        _buildCommand = buildCommandFactory.Create(this);
    }

    private Task Execute(InvocationContext ctx)
    {
        using var loggerFactory = CreateLoggerFactory(ctx);

        PrintOptions(loggerFactory, ctx);

        return _buildCommand.Execute(loggerFactory, ctx);
    }
}
