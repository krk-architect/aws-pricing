namespace Pricing.Strategies;

/// <summary>
///     Display strategy for text output
/// </summary>
public class TextDisplayStrategy : IDisplayStrategy
{
    public TextDisplayStrategy(int taskIndent = 0)
    {
        TaskIndent = taskIndent;
    }

    /// <summary>
    ///     Loaded configuration
    /// </summary>
    public Config Config { get; set; } = null!;

    /// <summary>
    ///     Indentation for tasks
    /// </summary>
    public int TaskIndent { get; }

    /// <summary>
    ///     StringBuilder to hold the formatted output
    /// </summary>
    private StringBuilder Sb { get; } = new();

    public void StartDocument(Config config) => Config = config;

    public void StartCluster(int clusterIdx, int clusterCount) { }

    public void FormatCluster(Cluster cluster)
    {
        Sb.AppendLine($"{cluster.Name}: {cluster.Cpu:0} vCPU, {cluster.Gb:0} GB, {cluster.Tasks.TotalTasks:#,##0} tasks ({cluster.Tasks.SavingsPlanTasks:#,##0} SP, {cluster.Tasks.OnDemandTasks:#,##0} OD)");
    }

    public void FormatTask(string type, ITask task)
    {
        var maxPriceLength = task.Cluster.TotalPrice.Year.Display.Length;

        var priceString = task.PricePer.Year.Display.PadLeft(maxPriceLength, ' ');
        Sb.AppendLine($"{new (' ', TaskIndent)}- ${priceString}   {task.Tasks} {type.PadRight(12)} tasks for {task.PerTaskHours,2} hours [{DateTime.Today.Add(task.StartTime),5:h tt} - {DateTime.Today.Add(task.EndTime),5:h tt})");
    }

    public void FormatClusterPrice(Cluster cluster)
    {
        var maxPriceLength = cluster.TotalPrice.Year.Display.Length;
        var priceString    = cluster.TotalPrice.Year.Display.PadLeft(maxPriceLength, ' ');
        Sb.AppendLine($"{new (' ', TaskIndent)}  {new ('=', priceString.Length + 1)}");
        Sb.AppendLine($"{new (' ', TaskIndent)}  ${priceString}");
    }

    public void EndCluster(bool isLastCluster)
    {
        if (isLastCluster)
        {
            return;
        }

        Sb.AppendLine();
    }

    public void EndDocument()
    {
        Sb.AppendLine();
        Sb.AppendLine($"SUM: ${Config.Price.Year.Display}");
        Sb.AppendLine();
    }

    public string GetResult() => Sb.ToString();
}
