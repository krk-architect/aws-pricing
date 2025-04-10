namespace Pricing.Models;

/// <summary>
///     Represents a <see cref="Currency" /> value for various time periods.
/// </summary>
public class PricePer
{
    /// <summary>
    ///     Construct a new <see cref="PricePer" /> object with a price per day.
    /// </summary>
    /// <param name="day"></param>
    public PricePer(decimal day)
    {
        AddDailyPrice(new (day));
    }

    /// <summary>
    ///     Price per hour.
    /// </summary>
    public Currency Hour  { get; private set; }

    /// <summary>
    ///     Price per day.
    /// </summary>
    public Currency Day   { get; private set; }

    /// <summary>
    ///     Price per month.
    /// </summary>
    public Currency Month { get; private set; }

    /// <summary>
    ///     Price per year.
    /// </summary>
    public Currency Year  { get; private set; }

    /// <summary>
    ///     Adds a daily price to the current price per day, hour, month and year.
    /// </summary>
    /// <param name="day"></param>
    public void AddDailyPrice(Currency day)
    {
        Day   = new (Day.Value + day.Value);
        Hour  = new (Day.Value  / 24);
        Year  = new (Day.Value  * 365.25m);
        Month = new (Year.Value / 12);
    }
}
