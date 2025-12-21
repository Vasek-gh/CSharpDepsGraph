using CSharpDepsGraph.Cli.Options;
using CSharpDepsGraph.Transforming;
using CSharpDepsGraph.Transforming.Filtering;

namespace CSharpDepsGraph.Cli.Commands;

public class TransformerBuilder
{
    private readonly List<INodeFilter> _filters;

    private readonly List<ITransformer> _transformers;

    public TransformerBuilder()
    {
        _filters = new List<INodeFilter>();
        _transformers = new List<ITransformer>();
    }

    public ITransformer Build()
    {
        var transformers = new List<ITransformer>();
        transformers.Add(new FlattenNamespacesTransformer());
        transformers.AddRange(_transformers);
        transformers.Add(new FilterTransformer(_filters));
        //transformers.Add(new LinkValidator()); todo optional

        return new CompositeTransformer(transformers);
    }

    public TransformerBuilder WithExternalHide(bool enabled)
    {
        if (enabled)
        {
            _transformers.Add(new ExternalHideTransformer());
        }

        return this;
    }

    public TransformerBuilder WithFilter(INodeFilter filter)
    {
        _filters.Add(filter);

        return this;
    }

    public TransformerBuilder WithExportLevel(NodeExportLevel nodeExportLevel, bool levelOnlyMode)
    {
        if (levelOnlyMode)
        {
            ITransformer transformer = nodeExportLevel switch
            {
                NodeExportLevel.Default => new AssemblyOnlyTransformer(),
                NodeExportLevel.Assembly => new AssemblyOnlyTransformer(),
                NodeExportLevel.Namespace => new NamespaceOnlyTransformer(),
                _ => throw new NotSupportedException()
            };

            _transformers.Add(transformer);
        }
        else
        {
            var filter = nodeExportLevel switch
            {
                NodeExportLevel.Assembly => Filters.DissolveNamespaces,
                NodeExportLevel.Namespace => Filters.DissolveTypes,
                NodeExportLevel.Type => Filters.DissolveMembers,
                NodeExportLevel.PublicMember => Filters.HidePrivate,
                _ => Filters.Empty
            };

            _transformers.Add(new FilterTransformer(filter));
        }

        return this;
    }

    public TransformerBuilder WithSymbolFilters(IEnumerable<NodeFilter> symbolFilters)
    {
        foreach (var filter in symbolFilters)
        {
            _filters.Add(new GlobFilter(filter.FilterAction, filter.Pattern));
        }

        return this;
    }

    public TransformerBuilder WithTransformer(ITransformer? transformer)
    {
        if (transformer != null)
        {
            _transformers.Add(transformer);
        }

        return this;
    }
}