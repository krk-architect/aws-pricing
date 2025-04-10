namespace Pricing.Models;

/// <summary>
///     Represents a <see cref="Value" /> with high precision for calculations, a <see cref="ValueRounded2" /> value
///     rounded to two decimal places and a <see cref="Display" /> string.
/// </summary>
public struct Currency
{
    /// <summary>
    ///     The value to be used for calculations.
    /// </summary>
    public decimal Value         { get; }

    /// <summary>
    ///     The value rounded to two decimal places.
    /// </summary>
    public decimal ValueRounded2 { get; }

    /// <summary>
    ///     The display string formatted with two decimal places.
    /// </summary>
    public string  Display       { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Currency" /> struct with the specified value.
    /// </summary>
    /// <param name="value"></param>
    public Currency(decimal value)
    {
        Value         = value;
        ValueRounded2 = Math.Round(value, 2, MidpointRounding.AwayFromZero);
        Display       = value.ToString("#,##0.00");
    }

    /// <summary>
    ///     Overload for multiplication with a decimal multiplier.
    /// </summary>
    /// <param name="currency"></param>
    /// <param name="multiplier"></param>
    /// <returns></returns>
    public static Currency operator *(Currency currency, decimal multiplier)
    {
        return new (currency.Value * multiplier);
    }

    /// <summary>
    ///     Overload for addition with another Currency.
    /// </summary>
    /// <param name="c1"></param>
    /// <param name="c2"></param>
    /// <returns></returns>
    public static Currency operator +(Currency c1, Currency c2)
    {
        return new (c1.Value + c2.Value);
    }

    /// <summary>
    ///     Custom deserialization from YAML.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static Currency FromYaml(string value)
    {
        if (decimal.TryParse(value, out var result))
        {
            return new (result);
        }

        throw new FormatException($"Invalid Currency format: {value}");
    }
}

/// <summary>
///     Custom deserialization for <see cref="Currency" /> from YAML.
/// </summary>
public class CurrencyYamlTypeConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(Currency);
    }

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var scalar = parser.Consume<Scalar>();
        return Currency.FromYaml(scalar.Value);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        throw new NotImplementedException(); // we're not writing yaml
    }
}
