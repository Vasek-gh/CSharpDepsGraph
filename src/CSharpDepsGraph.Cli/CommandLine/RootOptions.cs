using CSharpDepsGraph.Cli.Options;
using System.CommandLine;

namespace CSharpDepsGraph.Cli.CommandLine;

internal class RootOptions
{
    public static Argument<FileInfo> FileNameArgument { get; } = OptionBuilder.Create(() =>
    {
        var description = @"sln or slnx file";

        var fileNameArgument = new Argument<FileInfo>("filename");
        fileNameArgument.Description = description;
        fileNameArgument.HelpName = "solution";
        fileNameArgument.Validators.Add(result =>
        {
            var fileName = result.GetRequiredValue(fileNameArgument).FullName;
            var error = OptionsUtils.GetFileNameError(fileName);
            if (error is not null)
            {
                result.AddError(error);
            }
        });

        return fileNameArgument;
    });

    public static Option<Verbosity> VerbosityOption { get; } = OptionBuilder.Create(() =>
    {
        return OptionBuilder.CreateEnumOption<Verbosity>(
            "verbosity",
            "v",
            "level",
            "Sets the verbosity level of the command.",
            Verbosity.Normal,
            [
                ( Verbosity.Quiet, "q", "q[uiet]" ),
                ( Verbosity.Minimal, "m", "m[inimal]" ),
                ( Verbosity.Normal, "n", "n[ormal]" ),
                ( Verbosity.Detailed, "d", "d[etailed]" ),
                ( Verbosity.Diagnostic, "diag", "diag[nostic]" )
            ]
        );
    });

    public static Option<IEnumerable<KeyValuePair<string, string>>> PropertiesOption { get; } = OptionBuilder.Create(() =>
    {
        var description = @"
            Defines one or more MSBuild properties. Specify multiple properties delimited by
            semicolons or by repeating the option: -p prop1=val1;prop2=val2 or -p prop1=val1 -p prop2=val2.
        ";

        return OptionBuilder.CreateListOption<KeyValuePair<string, string>>(
            "property",
            "p",
            description,
            "name=value",
            argResult =>
            {
                var items = new List<KeyValuePair<string, string>>();

                foreach (var token in argResult.Tokens.SelectMany(t => t.Value.Split(";")))
                {
                    var propParts = token.Split("=").Select(s => s.Trim()).ToArray();
                    if (propParts.Length != 2
                        || string.IsNullOrWhiteSpace(propParts[0])
                        || string.IsNullOrWhiteSpace(propParts[1])
                        )
                    {
                        return ([], $"Invalid property format: {token}");
                    }

                    items.Add(new KeyValuePair<string, string>(propParts[0], propParts[1]));
                }

                return (items, null);
            }
        );
    });

    public static Option<bool> ParseGeneratedCodeOption { get; } = OptionBuilder.Create(() =>
    {
        return OptionBuilder.CreateOption<bool>(
            "parse-generated",
            null,
            "Parse the generated code not located in intermediate output path"
            );
    });

    public static Option<bool> CreateLinksToSelfOption { get; } = OptionBuilder.Create(() =>
    {
        return OptionBuilder.CreateOption<bool>(
            "links-to-self",
            null,
            "Include references to symbols from your own type"
            );
    });

    public static Option<bool> CreateLinksToPrimitiveTypesOption { get; } = OptionBuilder.Create(() =>
    {
        return OptionBuilder.CreateOption<bool>(
            "links-to-primitives",
            null,
            "Include links to symbols of primitive types"
            );
    });

    public static Option<bool> SplitAssembliesVersionsOption { get; } = OptionBuilder.Create(() =>
    {
        return OptionBuilder.CreateOption<bool>(
            "split-asm-versions",
            null,
            "Create separate nodes for each version of an assembly"
            );
    });
}