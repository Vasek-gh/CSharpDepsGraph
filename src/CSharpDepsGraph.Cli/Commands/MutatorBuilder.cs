using CSharpDepsGraph.Cli.Options;
using CSharpDepsGraph.Transforming;
using CSharpDepsGraph.Transforming.Filtering;

namespace CSharpDepsGraph.Cli.Commands;

public class MutatorBuilder
{
    private readonly List<INodeFilter> _filters;

    private readonly List<ITransformer> _mutators;

    public MutatorBuilder()
    {
        _filters = new List<INodeFilter>();
        _mutators = new List<ITransformer>();
    }

    public ITransformer Build()
    {
        var mutators = new List<ITransformer>();
        mutators.Add(new FlattenNamespacesTransformer());
        mutators.AddRange(_mutators);
        mutators.Add(new FilterTransformer(_filters));
        //mutators.Add(new LinkValidator()); todo optional

        return new CompositeTransformer(mutators);
    }

    public MutatorBuilder WithExternalHide(bool enabled)
    {
        if (enabled)
        {
            _mutators.Add(new ExternalHideTransformer());
        }

        return this;
    }

    public MutatorBuilder WithFilter(INodeFilter filter)
    {
        _filters.Add(filter);

        return this;
    }

    public MutatorBuilder WithExportLevel(NodeExportLevel nodeExportLevel, bool levelOnlyMode)
    {
        if (levelOnlyMode)
        {
            ITransformer mutator = nodeExportLevel switch
            {
                NodeExportLevel.Default => new AssemblyOnlyTransformer(),
                NodeExportLevel.Assembly => new AssemblyOnlyTransformer(),
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

            _mutators.Add(new FilterTransformer(filter));
        }

        return this;
    }

    public MutatorBuilder WithSymbolFilters(IEnumerable<NodeFilter> symbolFilters)
    {
        foreach (var filter in symbolFilters)
        {
            _filters.Add(new GlobFilter(filter.FilterAction, filter.Pattern));
        }

        return this;
    }

    public MutatorBuilder WithMutator(ITransformer? mutator)
    {
        if (mutator != null)
        {
            _mutators.Add(mutator);
        }

        return this;
    }
}