namespace Pricing.Strategies;

/// <summary>
///     Interface for displaying ECS Fargate configuration.
/// </summary>
public interface IDisplayStrategy
{
    /// <summary>
    ///     A hook allowing for any initial processing to be done before any clusters are formatted
    /// </summary>
    void   StartDocument(Config config);

    /// <summary>
    ///     A hook allowing for any initial processing to be done before each clusters is formatted
    /// </summary>
    void   StartCluster(int clusterIdx, int clusterCount);

    /// <summary>
    ///     Format a <see cref="Cluster" /> for display
    /// </summary>
    /// <param name="cluster"></param>
    void   FormatCluster(Cluster cluster);

    /// <summary>
    ///     Format a <see cref="ITask" /> for display
    /// </summary>
    /// <param name="type"></param>
    /// <param name="task"></param>
    void   FormatTask(string type, ITask task);

    /// <summary>
    ///     Format the total price for a <see cref="Cluster" /> for display
    /// </summary>
    /// <param name="cluster"></param>
    void   FormatClusterPrice(Cluster cluster);

    /// <summary>
    ///     A hook allowing for any final processing to be done after each clusters is formatted
    /// </summary>
    void   EndCluster(bool isLastCluster);

    /// <summary>
    ///     A hook allowing for any final processing to be done after all clusters have been formatted
    /// </summary>
    void   EndDocument();

    /// <summary>
    ///     Get the formatted result of the display strategy for the entire configuration
    /// </summary>
    /// <returns></returns>
    string GetResult();
}
