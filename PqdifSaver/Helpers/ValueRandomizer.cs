/// <summary>
/// Provides methods for randomizing measurement values for testing or simulation purposes.
/// </summary>
public class ValueRandomizer
{
    private readonly Random _random = new();

    /// <summary>
    /// Adjusts a value by a random percentage (positive or negative) up to the specified maximum.
    /// </summary>
    /// <param name="value">The original value.</param>
    /// <param name="maxPercentage">Maximum adjustment as a fraction (default 0.1).</param>
    /// <returns>The adjusted value.</returns>
    public double AdjustValue(double value, double maxPercentage = 0.1)
    {
        int sign = _random.Next(0, 2) == 0 ? 1 : -1;
        double adjustmentFactor = 1 + sign * _random.NextDouble() * maxPercentage;
        return value * adjustmentFactor;
    }

    /// <summary>
    /// Adjusts a value by a random percentage and scales it as an integer (×10000).
    /// </summary>
    /// <param name="value">The original value.</param>
    /// <param name="maxPercentage">Maximum adjustment as a fraction (default 0.01).</param>
    /// <returns>The adjusted integer value.</returns>
    public int AdjustValueAsInt(double value, double maxPercentage = 0.01)
    {
        return (int)(AdjustValue(value, maxPercentage) * 10000);
    }
}