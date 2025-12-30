/// <summary>
/// Represents a single measurement record with timestamp, value, and series information.
/// </summary>
public class Measurement
{
    /// <summary>
    /// Gets or sets the recording identifier for this measurement.
    /// </summary>
    public required string RecordingId {get; set;}
    /// <summary>
    /// Gets or sets the timestamp of the measurement.
    /// </summary>
    public DateTime timestamp {get; set;}
    /// <summary>
    /// Gets or sets the measured value.
    /// </summary>
    public double Value {get; set;}
    /// <summary>
    /// Gets or sets the series identifier for this measurement.
    /// </summary>
    public int SeriesId {get; set;}
}