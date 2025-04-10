namespace Pricing.Models;

/// <summary>
///    Represents the results of a pricing calculation.
/// </summary>
public class Results
{
    public string   OutputPath     { get; init; } = null!;
    public string   ConfigName     { get; init; } = null!;
    public string   ConfigFullPath { get; init; } = null!;
    public string   TextFullPath   { get; init; } = null!;
    public string   JsonFullPath   { get; init; } = null!;
    public string   TextContent    { get; init; } = null!;
    public string   JsonContent    { get; init; } = null!;
    public PricePer Price          { get; init; } = null!;
}
