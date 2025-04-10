// ReSharper disable MemberCanBeMadeStatic.Local

namespace Pricing.Visitors;

/// <summary>
///     Visitor to set values for properties using the deserialized objects
/// </summary>
public class ConfigOnDeserializedVisitor
{
    public ConfigOnDeserializedVisitor(Config config)
    {
        Config = config;
    }

    public Config Config { get; }

    /// <summary>
    ///     Visit a <see cref="Cluster" />, create its <see cref="FargateTask" /> and visit all the cluster's tasks
    /// </summary>
    /// <param name="cluster"></param>
    public void VisitCluster(Cluster cluster)
    {
        cluster.FargateTask = FargateTask.Create(cluster.Cpu, cluster.Gb, Config.Discounts.SavingsPlan.Value, Config.Discounts.Enterprise.Value);

        foreach (var task in cluster.Tasks.SavingsPlan)
        {
            VisitSavingsPlanTask(task);

            task.Cluster = cluster;
        }

        foreach (var task in cluster.Tasks.OnDemand)
        {
            VisitOnDemandTask(task);

            task.Cluster = cluster;
        }
    }

    /// <summary>
    ///     Visit a <see cref="SavingsPlanTask" /> and set the values for the properties
    /// </summary>
    /// <param name="task"></param>
    public void VisitSavingsPlanTask(SavingsPlanTask task)
    {
        task.Hours        = [0, 24];
        task.StartTime    = TimeSpan.FromHours(0);
        task.EndTime      = TimeSpan.FromHours(24);
        task.PerTaskHours = 24;
        task.TotalHours   = task.Tasks * task.PerTaskHours;
    }

    /// <summary>
    ///     Visit an <see cref="OnDemandTask" /> and set the values for the properties
    /// </summary>
    /// <param name="task"></param>
    public void VisitOnDemandTask(OnDemandTask task)
    {
        task.StartTime    = TimeSpan.FromHours(task.Hours[0]);
        task.EndTime      = TimeSpan.FromHours(task.Hours[1]);
        task.PerTaskHours = CalculatePerTaskHours(task);
        task.TotalHours   = task.Tasks * task.PerTaskHours;
    }

    /// <summary>
    ///     Calculate the number of hours a task will run per day
    /// </summary>
    /// <param name="task"></param>
    /// <returns>Number of hours the task will run per day</returns>
    private int CalculatePerTaskHours(OnDemandTask task)
    {
        var startDateTime = DateTime.Today.Add(task.StartTime);
        var endDateTime   = DateTime.Today.Add(task.EndTime);

        if (task.EndTime < task.StartTime)
        {
            endDateTime = endDateTime.AddDays(1);
        }

        return (int)(endDateTime - startDateTime).TotalHours;
    }
}
