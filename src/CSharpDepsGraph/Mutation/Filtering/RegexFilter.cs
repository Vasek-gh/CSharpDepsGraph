using System.Text.RegularExpressions;

namespace CSharpDepsGraph.Mutation.Filtering;

public class RegexFilter : IFilter
{
    private readonly Regex _regex;
    private readonly FilterAction _action;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegexFilter"/> class.
    /// </summary>
    public RegexFilter(FilterAction action, string pattern)
    {
        _action = action;
        _regex = new Regex(pattern);
    }

    /// <inheritdoc/>
    public FilterAction Execute(INode parent, INode node)
    {
        var match = _regex.Match(node.Id);
        if (!match.Success)
        {
            return FilterAction.Skip;
        }

        if (node.Id != match.Value)
        {
            return FilterAction.Skip;
        }

        return _action;
    }
}