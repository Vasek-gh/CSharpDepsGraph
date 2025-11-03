using System.CommandLine;
using CSharpDepsGraph.Cli.Commands.Settings;
using CSharpDepsGraph.Mutation.Filtering;

namespace CSharpDepsGraph.Cli.CommandLine;

internal static class ExportOptions
{
    public static Option<bool> HideExternal { get; } = OptionBuilder.Create(() =>
    {
        return OptionBuilder.CreateOption<bool>(
            "hide-external",
            "he",
            "When enabled all external nodes with root hides"
            );
    });

    public static Option<FileInfo?> OutputFileName { get; } = OptionBuilder.Create(() =>
    {
        return OptionBuilder.CreateOption<FileInfo?>(
            "output",
            "o",
            "The file name where the export should be written"
            );
    });

    public static Option<NodeExportLevel> ExportLevelFull { get; } = OptionBuilder.Create(() =>
    {
        return OptionBuilder.CreateOption<NodeExportLevel>(
            "export-level",
            "el",
            "level",
            "Defines the level below which all nodes are excluded.",
            ExportSettings.Defaults.ExportLevelFull,
            Enum.GetValues<NodeExportLevel>()
        );
    });

    public static Option<NodeExportLevel> ExportLevelOneLevel { get; } = OptionBuilder.Create(() =>
    {
        return OptionBuilder.CreateOption<NodeExportLevel>(
            "export-level",
            "el",
            "level",
            "Defines the level of nodes to export.",
            ExportSettings.Defaults.ExportLevelOneLevel,
            [NodeExportLevel.Assembly, NodeExportLevel.Namespace]
        );
    });

    public static Option<IEnumerable<RegexSymbolFilter>> SymbolFilters { get; } = OptionBuilder.Create(() =>
    {
        var description = @"
            Defines one or more symbol filter.
            Regex is applied on the node id.
            The filter action can be 'hide', 'dissolve' or 'skip'.
            When a node is hidden, it is deleted along with all its connections.
            When a node dissolves, the node is hidden and its links are linked to its parent.
            When a node is skipped, the node remains as is.
            All filters are applied to the node one by one. When the filter is triggered, this chain is interrupted.
            ";

        return OptionBuilder.CreateListOption<RegexSymbolFilter>(
            "symbol-filter",
            "sf",
            description,
            "filter action,regex pattern",
            argResult =>
            {
                var items = new List<RegexSymbolFilter>();

                foreach (var token in argResult.Tokens.Select(t => t.Value))
                {
                    var commaIndex = token.IndexOf(',', StringComparison.InvariantCulture);
                    if (commaIndex < 0)
                    {
                        argResult.ErrorMessage = MakeSymbolFilterError(token);
                        return Array.Empty<RegexSymbolFilter>();
                    }

                    var filterActionSubToken = token.Substring(0, commaIndex).Trim();
                    var regExPatternSubToken = token.Substring(commaIndex + 1).Trim();

                    if (string.IsNullOrWhiteSpace(filterActionSubToken)
                        || string.IsNullOrWhiteSpace(regExPatternSubToken)
                        || !Enum.TryParse<FilterAction>(filterActionSubToken, true, out var filterAction)
                        )
                    {
                        argResult.ErrorMessage = MakeSymbolFilterError(token);
                        return Array.Empty<RegexSymbolFilter>();
                    }

                    items.Add(new RegexSymbolFilter()
                    {
                        FilterAction = filterAction,
                        RegExPattern = regExPatternSubToken
                    });
                }

                return items;
            }
        );

        string MakeSymbolFilterError(string token)
        {
            return $"Invalid symbol filter format: {token}";
        }
    });

    internal static class Json
    {
        public static Option<bool> Format { get; } = OptionBuilder.Create(() =>
        {
            return OptionBuilder.CreateOption<bool>(
                "format",
                null,
                "When enabled json writes formatted output")
                ;
        });
    }
}
