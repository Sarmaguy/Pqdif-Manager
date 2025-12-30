/// <summary>
/// Provides constant values for measurement processing, such as harmonic limits and sample sizes.
/// </summary>
public static class MeasurementConstants
{
    /// <summary>
    /// The maximum number of harmonics supported.
    /// </summary>
    public const int MaxHarmonics = 63;
    /// <summary>
    /// The maximum number of interharmonics supported.
    /// </summary>
    public const int MaxInterharmonics = 50;
    /// <summary>
    /// The number of recordings per measurement.
    /// </summary>
    public const int RecordingsPerMeasurement = 120;
    /// <summary>
    /// The sample size for 60 Hz frequency measurements.
    /// </summary>
    public const int FrequencySampleSize60 = 60;
    /// <summary>
    /// The sample size for 720 Hz frequency measurements.
    /// </summary>
    public const int FrequencySampleSize720 = 720;
}