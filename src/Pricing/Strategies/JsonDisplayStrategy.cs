namespace Pricing.Strategies;

/// <summary>
///     Display strategy for JSON output
/// </summary>
public class JsonDisplayStrategy : IDisplayStrategy
{
    /// <summary>
    ///     Loaded configuration
    /// </summary>
    public Config Config { get; set; } = null!;

    /// <summary>
    ///     Root <see cref="JsonObject" /> to build the JSON object for display
    /// </summary>
    private JsonObject Root { get; } = new();

    /// <summary>
    ///     Current <see cref="JsonArray" /> for the cluster being formatted
    /// </summary>
    private JsonArray? CurrentCluster { get; set; }

    public void StartDocument(Config config) => Config = config;

    public void StartCluster(int clusterIdx, int clusterCount) { }

    public void FormatCluster(Cluster cluster)
    {
        CurrentCluster = [];
        Root[cluster.Name] = new JsonObject
                             {
                                 ["cpu"]        = cluster.Cpu
                               , ["gb"]         = cluster.Gb
                               , ["totalTasks"] = cluster.Tasks.TotalTasks
                               , ["tasks"]      = CurrentCluster
                             };
    }

    public void FormatTask(string type, ITask task)
    {
        CurrentCluster?.Add(new JsonObject
                            {
                                ["type"]  = type
                              , ["tasks"] = task.Tasks
                              , ["hours"] = new JsonObject
                                            {
                                                ["start"]   = task.Hours[0]
                                              , ["end"]     = task.Hours[1]
                                              , ["perTask"] = (task.EndTime - task.StartTime).TotalHours
                                              , ["total"]   = (task.EndTime - task.StartTime).TotalHours * task.Tasks
                                            }
                              , ["price"] = new JsonObject
                                            {
                                                ["year"]  = task.PricePer.Year.ValueRounded2
                                              , ["month"] = task.PricePer.Month.ValueRounded2
                                              , ["day"]   = task.PricePer.Day.ValueRounded2
                                              , ["hour"]  = task.PricePer.Hour.ValueRounded2
                                            }
                            });
    }

    public void FormatClusterPrice(Cluster cluster)
    {
        Root[cluster.Name]!["price"] = new JsonObject
                                       {
                                           ["year"]  = cluster.TotalPrice.Year.ValueRounded2
                                         , ["month"] = cluster.TotalPrice.Month.ValueRounded2
                                         , ["day"]   = cluster.TotalPrice.Day.ValueRounded2
                                         , ["hour"]  = cluster.TotalPrice.Hour.ValueRounded2
                                       };
    }

    public void EndCluster(bool isLastCluster) { }

    public void EndDocument()
    {
        Root["sum"] = new JsonObject
                      {
                          ["year"]  = Config.Price.Year.ValueRounded2
                        , ["month"] = Config.Price.Month.ValueRounded2
                        , ["day"]   = Config.Price.Day.ValueRounded2
                        , ["hour"]  = Config.Price.Hour.ValueRounded2
                      };
    }

    public string GetResult() => Root.ToJsonString(new()
                                                   {
                                                       WriteIndented = true
                                                     , IndentSize    = 4
                                                   });
}
