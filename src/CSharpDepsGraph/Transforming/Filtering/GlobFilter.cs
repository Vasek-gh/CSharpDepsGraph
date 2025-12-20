using Microsoft.Extensions.FileSystemGlobbing;

namespace CSharpDepsGraph.Transforming.Filtering;

/// <summary>
/// Glob pattern filter
/// </summary>
public class GlobFilter : INodeFilter
{
    private readonly Matcher _matcher;
    private readonly FilterAction _action;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobFilter"/> class.
    /// </summary>
    public GlobFilter(FilterAction action, string pattern)
    {
        _action = action;
        _matcher = new Matcher();
        _matcher.AddInclude(pattern);
    }

    /// <inheritdoc/>
    public FilterAction Execute(NodeContext context)
    {
        var matchingResult = _matcher.Match(".", context.Path);
        if (!matchingResult.HasMatches)
        {
            return FilterAction.Skip;
        }

        return _action;
    }
}