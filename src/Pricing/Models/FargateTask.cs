namespace Pricing;

/// <summary>
///     Represents a Fargate task with a specific CPU and memory configuration.
/// </summary>
public class FargateTask : IEquatable<FargateTask>
{
    public const decimal HourlyPriceCpu      = 0.04048m;  // 1 vCPU
    public const decimal HourlyPriceGb       = 0.004445m; // 1 GB
    public const decimal DiscountOnDemand    = 0.0m;      // no discount
    public const decimal DiscountSavingsPlan = 0.2m;      // 1 year, no up front
    public const decimal DiscountEnterprise  = 0.15m;     // applied after all other discounts

    /// <summary>
    ///     Static constructor to initialize the combinations of CPU and memory for Fargate tasks.
    /// </summary>
    static FargateTask()
    {
        // @formatter:off

        Combinations =
        [
            new(0.25, 0.5)
          , new(0.25, 1  )
          , new(0.25, 2  )
        ];

        Seq( 1,   4, 1).ForEach(static m => Combinations.Add(new( 0.5, m)));
        Seq( 2,   8, 1).ForEach(static m => Combinations.Add(new( 1  , m)));
        Seq( 4,  16, 1).ForEach(static m => Combinations.Add(new( 2  , m)));
        Seq( 8,  30, 1).ForEach(static m => Combinations.Add(new( 4  , m)));
        Seq(16,  60, 4).ForEach(static m => Combinations.Add(new( 8  , m)));
        Seq(32, 120, 8).ForEach(static m => Combinations.Add(new(16  , m)));

        // @formatter:on
    }

    /// <summary>
    ///     Constructor to create a Fargate task with the specified CPU and memory configuration.
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="gb"></param>
    /// <param name="discountSavingsPlan"></param>
    /// <param name="discountEnterprise"></param>
    private FargateTask(double cpu, double gb, decimal discountSavingsPlan = DiscountSavingsPlan, decimal discountEnterprise = DiscountEnterprise)
    {
        Cpu = cpu;
        Gb  = gb;

        var onDemandPricePerHour    = CalculatePricePerHour(cpu, gb, DiscountOnDemand,    discountEnterprise);
        var savingsPlanPricePerHour = CalculatePricePerHour(cpu, gb, discountSavingsPlan, discountEnterprise);

        OnDemandPricePer    = new(onDemandPricePerHour    * 24);
        SavingsPlanPricePer = new(savingsPlanPricePerHour * 24);
    }

    /// <summary>
    ///     Static property to hold all valid combinations of CPU and memory for Fargate tasks.
    /// </summary>
    public static HashSet<FargateTask> Combinations { get; }

    /// <summary>
    ///     CPU configuration of the Fargate task in vCPUs.
    /// </summary>
    public double Cpu { get; }

    /// <summary>
    ///     Memory configuration of the Fargate task in GB.
    /// </summary>
    public double Gb  { get; }

    /// <summary>
    ///     On-demand price for the Fargate task represented in various timeframes.
    /// </summary>
    public PricePer OnDemandPricePer    { get; }

    /// <summary>
    ///     Savings plan price for the Fargate task represented in various timeframes.
    /// </summary>
    public PricePer SavingsPlanPricePer { get; }

    #region IEquatable<FargateTask>

    /// <summary>
    ///     Implementation of the <see cref="IEquatable{T}" /> interface to compare two FargateTask objects.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(FargateTask? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Math.Abs(Cpu - other.Cpu) < 0.000001 && Math.Abs(Gb - other.Gb) < 0.000001;
    }

    #endregion

    /// <summary>
    ///     Override the ToString method to provide a string representation of the Fargate task.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"CPU={Cpu}  GB={Gb,2}  OnDemand={OnDemandPricePer.Hour.Value:0.00000}  SavingsPlan={SavingsPlanPricePer.Hour.Value:0.00000}";
    }

    /// <summary>
    ///     Create a Fargate task with the specified CPU and memory configuration.
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="gb"></param>
    /// <param name="discountSavingsPlan"></param>
    /// <param name="discountEnterprise"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static FargateTask Create(double cpu, double gb, decimal discountSavingsPlan, decimal discountEnterprise)
    {
        if (!Combinations.TryGetValue(new (cpu, gb), out _))
        {
            throw new ArgumentException($"Invalid combination of CPU and memory: {cpu} CPU, {gb} GB");
        }

        return new (cpu, gb, discountSavingsPlan, discountEnterprise);
    }

    /// <summary>
    ///     Calculate the price per hour for a Fargate task based on its CPU and memory configuration.
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="gb"></param>
    /// <param name="discount"></param>
    /// <param name="enterpriseDiscount"></param>
    /// <returns></returns>
    private static decimal CalculatePricePerHour(double cpu, double gb, decimal discount, decimal enterpriseDiscount)
    {
        var pricePerHourCpu = (decimal)cpu * HourlyPriceCpu;
        var pricePerHourGb  = (decimal)gb  * HourlyPriceGb;
        var pricePerHour    = pricePerHourCpu + pricePerHourGb;

        pricePerHour *= 1 - discount;
        pricePerHour *= 1 - enterpriseDiscount;

        return Math.Round(pricePerHour, 5);
    }

    /// <summary>
    ///     Display the combinations of CPU and memory for Fargate tasks in a table format for the console.
    /// </summary>
    public static void PrintCombinationsTable()
    {
        Console.WriteLine("|   CPU   |   GB    | On Demand | Savings Plan |");
        Console.WriteLine("|---------|---------|-----------|--------------|");

        double? previousCpu = null;

        foreach (var task in Combinations.OrderBy(static t => t.Cpu).ThenBy(static t => t.Gb))
        {
            if (previousCpu.HasValue && Math.Abs(previousCpu.Value - task.Cpu) > 0.000001)
            {
                Console.WriteLine("|---------|---------|-----------|--------------|");
            }

            var cpuStr = task.Cpu % 1 == 0 ? $"{(int)task.Cpu}   " : $"{(int)task.Cpu}.{(int)(task.Cpu % 1 * 100):D2}";
            var gbStr  = task.Gb  % 1 == 0 ? $"{(int)task.Gb}   " : $"{(int)task.Gb}.{(int)(task.Gb    % 1 * 100):D2}";

            var onDemandStr    = task.OnDemandPricePer.Hour.Value.ToString("F5");
            var savingsPlanStr = task.SavingsPlanPricePer.Hour.Value.ToString("F5");

            Console.WriteLine($"| {cpuStr,7} | {gbStr,7} |   {onDemandStr,7} |      {savingsPlanStr,7} |");

            previousCpu = task.Cpu;
        }
    }

    /// <summary>
    ///     Generate a sequence of numbers.
    /// </summary>
    /// <param name="first">First number in generated sequence</param>
    /// <param name="last">Last number in generated sequence</param>
    /// <param name="step">Distance between each number in generated sequence</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static double[] Seq(double first, double last, double step)
    {
        if (step == 0)
        {
            throw new ArgumentException("Step cannot be zero", nameof(step));
        }

        // Calculate how many elements we need
        int count;
        if (step > 0 && first <= last)
        {
            count = (int)Math.Floor((last - first) / step) + 1;
        }
        else if (step < 0 && first >= last)
        {
            count = (int)Math.Floor((first - last) / -step) + 1;
        }
        else
        {
            return [];
        }

        var result = new double[count];
        for (var i = 0; i < count; i++)
        {
            result[i] = first + i * step;
        }

        return result;
    }

    #region Overrides required for HashSet

    public override bool Equals(object? obj)
    {
        return obj is FargateTask other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Cpu, Gb);
    }

    #endregion
}

public static class Extensions
{
    /// <summary>
    ///     Iterate over each element in an array and perform an action.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="action"></param>
    public static void ForEach<T>(this T[]? @this, Action<T> action)
    {
        if (@this == null)
        {
            return;
        }

        foreach (var item in @this)
        {
            action(item);
        }
    }
}
