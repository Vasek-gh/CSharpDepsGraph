using CSharpDepsGraph.Cli.Options;
using CSharpDepsGraph.Mutation;
using CSharpDepsGraph.Mutation.Filtering;

namespace CSharpDepsGraph.Cli.Commands;

internal class MutatorBuilder
{
    private readonly List<IFilter> _filters;

    private readonly List<IMutator> _mutators;

    public MutatorBuilder()
    {
        _filters = new List<IFilter>();
        _mutators = new List<IMutator>();
    }

    public IMutator Build()
    {
        var mutators = new List<IMutator>();
        mutators.Add(new FlattenNamespacesMutator());
        mutators.AddRange(_mutators);
        mutators.Add(new FilterMutator(_filters));
        //mutators.Add(new LinkValidator()); todo optional

        return new CompositeMutator(mutators);
    }

    public MutatorBuilder WithExternalHide(bool enabled)
    {
        if (enabled)
        {
            _mutators.Add(new ExternalHideMutator());
        }

        return this;
    }

    public MutatorBuilder WithFilter(IFilter filter)
    {
        _filters.Add(filter);

        return this;
    }

    public MutatorBuilder WithRegexFilter(RegexSymbolFilter filter)
    {
        _filters.Add(new RegexFilter(filter.FilterAction, filter.RegExPattern));

        return this;
    }

    public MutatorBuilder WithExportLevel(NodeExportLevel nodeExportLevel, bool levelOnlyMode)
    {
        if (levelOnlyMode)
        {
            IMutator mutator = nodeExportLevel switch
            {
                NodeExportLevel.Default => new AssemblyOnlyMutator(),
                NodeExportLevel.Assembly => new AssemblyOnlyMutator(),
                NodeExportLevel.Namespace => new NamespaceOnlyTransformer(),
                _ => throw new NotSupportedException()
            };

            _mutators.Add(mutator);
        }
        else
        {
            var filter = nodeExportLevel switch
            {
                NodeExportLevel.Assembly => Filters.HideNamespaces,
                NodeExportLevel.Namespace => Filters.HideTypes,
                NodeExportLevel.Type => Filters.HideMembers,
                NodeExportLevel.PublicMember => Filters.HidePrivate,
                _ => Filters.Empty
            };

            _mutators.Add(new FilterMutator(filter));
        }

        return this;
    }

    public MutatorBuilder WithSymbolFilters(IEnumerable<RegexSymbolFilter> symbolFilters)
    {
        foreach (var filter in symbolFilters)
        {
            _filters.Add(new RegexFilter(filter.FilterAction, filter.RegExPattern));
        }

        return this;
    }

    public MutatorBuilder WithMutator(IMutator? mutator)
    {
        if (mutator != null)
        {
            _mutators.Add(mutator);
        }

        return this;
    }
}