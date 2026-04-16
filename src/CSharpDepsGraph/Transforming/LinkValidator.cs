namespace CSharpDepsGraph.Transforming;

/// <summary>
/// Transformer to validate that all links are attached to existing nodes
/// </summary>
public class LinkValidator : ITransformer
{
    /// <inheritdoc/>
    public IGraph Execute(IGraph graph)
    {
        var nodeMap = graph.Root.CollectChildNodes().ToDictionary(n => n.Uid);

        foreach (var link in graph.Links)
        {
            if (!nodeMap.ContainsKey(link.Source.Uid) || !nodeMap.ContainsKey(link.Target.Uid))
            {
                throw new CSharpDepsGraphException($"""
                    Detect corrupted link:
                        SourceId: {link.Source.Uid}
                        TargetId: {link.Target.Uid}
                        OriginalSourceId: {link.OriginalSource.Uid}
                        OriginalTargetId: {link.OriginalTarget.Uid}
                    """);
            }
        }

        return graph;
    }
}