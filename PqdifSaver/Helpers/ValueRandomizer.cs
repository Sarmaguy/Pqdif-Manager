public class ValueRandomizer
{
    private readonly Random _random = new();

    public double AdjustValue(double value, double maxPercentage = 0.1)
    {
        int sign = _random.Next(0, 2) == 0 ? 1 : -1;
        double adjustmentFactor = 1 + sign * _random.NextDouble() * maxPercentage;
        return value * adjustmentFactor;
    }

    public int AdjustValueAsInt(double value, double maxPercentage = 0.01)
    {
        return (int)(AdjustValue(value, maxPercentage) * 10000);
    }
}