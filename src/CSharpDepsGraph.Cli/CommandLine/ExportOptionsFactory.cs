using System.CommandLine;
using CSharpDepsGraph.Cli.Options;
using CSharpDepsGraph.Transforming.Filtering;

namespace CSharpDepsGraph.Cli.CommandLine;

internal static class ExportOptionsFactory
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
        return OptionBuilder.CreateEnumOption<NodeExportLevel>(
            "export-level",
            "el",
            "level",
            "Defines the level below which all nodes are excluded.",
            NodeExportLevel.All,
            Enum.GetValues<NodeExportLevel>().Where(e => e > NodeExportLevel.Default).ToArray()
        );
    });

    public static Option<NodeExportLevel> ExportLevelOneLevel { get; } = OptionBuilder.Create(() =>
    {
        return OptionBuilder.CreateEnumOption<NodeExportLevel>(
            "export-level",
            "el",
            "level",
            "Defines the level of nodes to export.",
            NodeExportLevel.Assembly,
            [NodeExportLevel.Assembly, NodeExportLevel.Namespace]
        );
    });

    public static Option<IEnumerable<RegexSymbolFilter>> SymbolFilters { get; } = OptionBuilder.Create(() =>
    {
        // todo Regex is applied on the node path not id
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
                        return ([], MakeSymbolFilterError(token));
                    }

                    var filterActionSubToken = token.Substring(0, commaIndex).Trim();
                    var regExPatternSubToken = token.Substring(commaIndex + 1).Trim();

                    if (string.IsNullOrWhiteSpace(filterActionSubToken)
                        || string.IsNullOrWhiteSpace(regExPatternSubToken)
                        || !Enum.TryParse<FilterAction>(filterActionSubToken, true, out var filterAction)
                        )
                    {
                        return ([], MakeSymbolFilterError(token));
                    }

                    items.Add(new RegexSymbolFilter()
                    {
                        FilterAction = filterAction,
                        RegExPattern = regExPatternSubToken
                    });
                }

                if (items.Count == 0)
                {
                    return ([], "Empty symbol-filter");
                }

                return (items, null);
            }
        );

        string MakeSymbolFilterError(string token)
        {
            return $"Invalid symbol-filter format: {token}";
        }
    });

    internal static class Json
    {
        public static Option<bool> Format { get; } = OptionBuilder.Create(() =>
        {
            return OptionBuilder.CreateOption<bool>(
                "format",
                null,
                "When enabled json writes formatted output"
                );
        });
    }
}
