// ReSharper disable MemberCanBeMadeStatic.Global

namespace Pricing.Visitors;

/// <summary>
///     Visitor pattern for calculating the price of ECS Fargate tasks.
/// </summary>
public class CalculatePriceVisitor
{
    /// <summary>
    ///     Visit a <see cref="Cluster" />, visit all the cluster's tasks and calculate the total price for the cluster
    /// </summary>
    /// <param name="cluster"></param>
    public void VisitCluster(Cluster cluster)
    {
        cluster.TotalSavingsPlanPrice = new(0);
        cluster.TotalOnDemandPrice    = new(0);

        foreach (var task in cluster.Tasks.SavingsPlan)
        {
            VisitSavingsPlanTask(task, cluster.FargateTask);

            cluster.TotalSavingsPlanPrice.AddDailyPrice(task.PricePer.Day);
        }

        foreach (var task in cluster.Tasks.OnDemand)
        {
            VisitOnDemandTask(task, cluster.FargateTask);

            cluster.TotalOnDemandPrice.AddDailyPrice(task.PricePer.Day);
        }

        cluster.TotalPrice = new(cluster.TotalSavingsPlanPrice.Day.Value + cluster.TotalOnDemandPrice.Day.Value);
    }

    /// <summary>
    ///     Visit a <see cref="SavingsPlanTask" /> and calculate the price based on the <see cref="FargateTask" />
    /// </summary>
    /// <param name="task"></param>
    /// <param name="fargateTask"></param>
    public void VisitSavingsPlanTask(SavingsPlanTask task, FargateTask fargateTask)
    {
        task.PricePer = new(fargateTask.SavingsPlanPricePer.Hour.Value * task.TotalHours);

        task.AnnualPriceString = task.PricePer.Year.Display;
    }

    /// <summary>
    ///     Visit an <see cref="OnDemandTask" /> and calculate the price based on the <see cref="FargateTask" />
    /// </summary>
    /// <param name="task"></param>
    /// <param name="fargateTask"></param>
    public void VisitOnDemandTask(OnDemandTask task, FargateTask fargateTask)
    {
        task.PricePer = new(fargateTask.OnDemandPricePer.Hour.Value * task.TotalHours);

        task.AnnualPriceString = task.PricePer.Year.Display;
    }
}
