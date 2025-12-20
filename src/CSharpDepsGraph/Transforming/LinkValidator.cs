namespace CSharpDepsGraph.Transforming;

/// <summary>
/// Transformer to validate that all links are attached to existing nodes
/// </summary>
public class LinkValidator : ITransformer
{
    /// <inheritdoc/>
    public IGraph Execute(IGraph graph)
    {
        var nodeMap = graph.Root.CollectChildNodes().ToDictionary(n => n.Id);

        foreach (var link in graph.Links)
        {
            if (!nodeMap.ContainsKey(link.Source.Id) || !nodeMap.ContainsKey(link.Target.Id))
            {
                throw new CSharpDepsGraphException($"""
                    Detect corrupted link:
                        SourceId: {link.Source.Id}
                        TargetId: {link.Target.Id}
                        OriginalSourceId: {link.OriginalSource.Id}
                        OriginalTargetId: {link.OriginalTarget.Id}
                    """);
            }
        }

        return graph;
    }
}