using System.CommandLine;
using CSharpDepsGraph.Cli.Options;
using CSharpDepsGraph.Transforming.Filtering;

namespace CSharpDepsGraph.Cli.CommandLine;

internal static class ExportOptionsFactory
{
    // todo надо пересмотреть эту опцию с учетом что у внешних теперь нет рута
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

    public static Option<IEnumerable<NodeFilter>> NodeFilters { get; } = OptionBuilder.Create(() =>
    {
        var description = @"
            Defines one or more node filter.
            Glob pattern is applied on the node path.
            The filter action can be 'hide', 'dissolve' or 'skip'.
            When a node is hidden, it is deleted along with all its connections.
            When a node dissolves, the node is hidden and its links are linked to its parent.
            When a node is skipped, the node remains as is.
            All filters are applied to the node one by one. When the filter is triggered, this chain is interrupted.
            ";

        return OptionBuilder.CreateListOption<NodeFilter>(
            "node-filter",
            "nf",
            description,
            "filter action,glob pattern",
            argResult =>
            {
                var items = new List<NodeFilter>();

                foreach (var token in argResult.Tokens.Select(t => t.Value))
                {
                    var commaIndex = token.IndexOf(',', StringComparison.InvariantCulture);
                    if (commaIndex < 0)
                    {
                        return ([], MakeSymbolFilterError(token));
                    }

                    var actionSubToken = token.Substring(0, commaIndex).Trim();
                    var patternSubToken = token.Substring(commaIndex + 1).Trim();

                    if (string.IsNullOrWhiteSpace(actionSubToken)
                        || string.IsNullOrWhiteSpace(patternSubToken)
                        || !Enum.TryParse<FilterAction>(actionSubToken, true, out var filterAction)
                        )
                    {
                        return ([], MakeSymbolFilterError(token));
                    }

                    items.Add(new NodeFilter()
                    {
                        FilterAction = filterAction,
                        Pattern = patternSubToken
                    });
                }

                if (items.Count == 0)
                {
                    return ([], "Empty node-filter");
                }

                return (items, null);
            }
        );

        string MakeSymbolFilterError(string token)
        {
            return $"Invalid node-filter format: {token}";
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

        public static Option<bool> ExcludeLocations { get; } = OptionBuilder.Create(() =>
        {
            return OptionBuilder.CreateOption<bool>(
                "exclude-locations",
                null,
                "When enabled, export do not write locations"
                );
        });

        public static Option<bool> InlinePaths { get; } = OptionBuilder.Create(() =>
        {
            return OptionBuilder.CreateOption<bool>(
                "inline-paths",
                null,
                "When this option is enabled, the path to the file is written directly to the location itself"
                );
        });



    }
}
