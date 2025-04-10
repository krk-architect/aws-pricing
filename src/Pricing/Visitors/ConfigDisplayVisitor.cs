namespace Pricing.Visitors;

/// <summary>
///     Visitor pattern for displaying configuration.
/// </summary>
public class ConfigDisplayVisitor
{
    public ConfigDisplayVisitor(Config config, IDisplayStrategy strategy)
    {
        Strategy = strategy;
        Strategy.StartDocument(config);
    }

    public IDisplayStrategy Strategy { get; }

    /// <summary>
    ///     Visit a <see cref="Cluster" />, use the display <see cref="Strategy" /> to format the cluster and visit all the
    ///     cluster's tasks
    /// </summary>
    /// <param name="cluster"></param>
    /// <param name="clusterIdx"></param>
    /// <param name="clusterCount"></param>
    public void VisitCluster(Cluster cluster, int clusterIdx, int clusterCount)
    {
        Strategy.StartCluster(clusterIdx, clusterCount);

        Strategy.FormatCluster(cluster);

        foreach (var task in cluster.Tasks.SavingsPlan)
        {
            VisitSavingsPlanTask(task);
        }

        foreach (var task in cluster.Tasks.OnDemand)
        {
            VisitOnDemandTask(task);
        }

        Strategy.FormatClusterPrice(cluster);

        Strategy.EndCluster(clusterIdx + 1 == clusterCount);
    }

    /// <summary>
    ///     Visit a <see cref="SavingsPlanTask" /> and use the display <see cref="Strategy" /> to format the task
    /// </summary>
    /// <param name="task"></param>
    public void VisitSavingsPlanTask(SavingsPlanTask task)
    {
        Strategy.FormatTask("Savings Plan", task);
    }

    /// <summary>
    ///     Visit an <see cref="OnDemandTask" /> and use the display <see cref="Strategy" /> to format the task
    /// </summary>
    /// <param name="task"></param>
    public void VisitOnDemandTask(OnDemandTask task)
    {
        Strategy.FormatTask("On Demand", task);
    }

    /// <summary>
    ///     Get the result of the display <see cref="Strategy" /> after all the clusters and tasks have been visited
    /// </summary>
    /// <returns>Formatted output based on the display <see cref="Strategy" /></returns>
    public string GetResult()
    {
        Strategy.EndDocument();
        return Strategy.GetResult();
    }
}
