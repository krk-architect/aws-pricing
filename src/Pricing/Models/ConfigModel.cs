namespace Pricing.Models;

public class Config
{
    public string    Region    { get; set; } = null!;
    public Discounts Discounts { get; set; } = null!;
    public Cluster[] Clusters  { get; set; } = [];

    public PricePer Price    { get; set; } = null!;

    public string Display(IDisplayStrategy strategy)
    {
        var visitor = new ConfigDisplayVisitor(this, strategy);
        for (var i = 0; i < Clusters.Length; i++)
        {
            visitor.VisitCluster(Clusters[i], i, Clusters.Length);
        }

        return visitor.GetResult();
    }

    #region File

    private FileInfo? _fileInfo;

    public FileInfo FileInfo
    {
        get => _fileInfo ?? throw new InvalidOperationException("FileInfo is not set");
        set
        {
            _fileInfo = value ?? throw new ArgumentNullException(nameof(value));

            Name = Path.GetFileNameWithoutExtension(value.Name);
        }
    }

    public string Name { get; private set; } = null!;

    #endregion
}

public class Discounts
{
    public Currency Enterprise  { get; set; }
    public Currency SavingsPlan { get; set; }
}

public class Cluster
{
    public string Name  { get; set; } = null!;
    public double Cpu   { get; set; }
    public double Gb    { get; set; }
    public Tasks  Tasks { get; set; } = null!;

    public FargateTask FargateTask { get; set; } = null!;

    public PricePer TotalSavingsPlanPrice { get; set; } = null!;
    public PricePer TotalOnDemandPrice    { get; set; } = null!;
    public PricePer TotalPrice            { get; set; } = null!;
}

public class Tasks
{
    public SavingsPlanTask[] SavingsPlan { get; set; } = [];
    public OnDemandTask[]    OnDemand    { get; set; } = [];

    public int SavingsPlanTasks => SavingsPlan.Sum(static sp => sp.Tasks);
    public int OnDemandTasks    => OnDemand.Sum(static od => od.Tasks);
    public int TotalTasks       => SavingsPlanTasks + OnDemandTasks;
}

public interface ITask
{
    int   Tasks { get; set; }
    int[] Hours { get; set; }

    TimeSpan StartTime { get; set; }
    TimeSpan EndTime   { get; set; }

    int PerTaskHours { get; set; }
    int TotalHours   { get; set; }

    PricePer PricePer { get; set; }

    string AnnualPriceString { get; set; }

    Cluster Cluster { get; set; }
}

public class SavingsPlanTask : ITask
{
    public int   Tasks { get; set; }
    public int[] Hours { get; set; } = [];

    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime   { get; set; }

    public int PerTaskHours { get; set; }
    public int TotalHours   { get; set; }

    public PricePer PricePer { get; set; } = null!;

    public string AnnualPriceString { get; set; } = null!;

    public Cluster Cluster { get; set; } = null!;
}

public class OnDemandTask : ITask
{
    public int   Tasks { get; set; }
    public int[] Hours { get; set; } = null!;

    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime   { get; set; }

    public int PerTaskHours { get; set; }
    public int TotalHours   { get; set; }

    public PricePer PricePer { get; set; } = null!;

    public string AnnualPriceString { get; set; } = null!;

    public Cluster Cluster { get; set; } = null!;
}
