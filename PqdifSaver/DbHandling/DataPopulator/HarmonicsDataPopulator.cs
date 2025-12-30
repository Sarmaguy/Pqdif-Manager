using System.Data;
using System.Text.RegularExpressions;
using PQDIF_Manager;

/// <summary>
/// Populates a DataTable with harmonic or interharmonic measurement data from a PQDIF file.
/// Handles extraction, normalization, and calculation of harmonic values for each channel and phase.
/// </summary>
public class HarmonicsDataPopulator : IDataPopulator
{
    private readonly ValueRandomizer _randomizer = new();
    private static Dictionary<string, IList<object>> FirstHarmonic =  new Dictionary<string, IList<object>>();

    /// <summary>
    /// Initializes the populator and caches first harmonic values for all relevant channels.
    /// </summary>
    /// <param name="pqdifFile">The PQDIF file to extract first harmonic data from.</param>
    public HarmonicsDataPopulator(PqdifFile pqdifFile)
    {
        Channel[] channels = pqdifFile.Channels;
        foreach (var channel in channels)
        {
            if (channel.ChannelName.Contains("Harmonic") && ExtractHarmonicIndex(channel.ChannelName) == 1)
            {
                Series series = channel.ValueSeries[0];
                var phase = PhaseConverter.ConvertPhase(
                    channel.Phase.ToString(), 
                    channel.QuantityMeasured.ToString()
                );

                FirstHarmonic[phase] = series.OriginalValues;
            }
        }
    }

    /// <summary>
    /// Populates the provided DataTable with harmonic/interharmonic data from the PQDIF file.
    /// </summary>
    /// <param name="table">The DataTable to populate.</param>
    /// <param name="pqdifFile">The PQDIF file containing measurement data.</param>
    public async Task PopulateAsync(DataTable table, PqdifFile pqdifFile)
    {
        var channels = pqdifFile.Channels;
        int totalMeasurements = channels[0].TimeSeries.SampleCount;
        DateTime startTime = pqdifFile.StartTime;
        string type = (table.Columns.Contains("U1H1") || table.Columns.Contains("U1IH1") )  ? "Voltage" : "Current";
        string harmonicType = (table.Columns.Contains("U1H1") || table.Columns.Contains("I1H1") )  ? "Harmonic" : "Interharmonic";


        for (int measurementIndex = 0; measurementIndex < totalMeasurements; measurementIndex++)
        {
            var timestamp = CalculateTimestamp(startTime, channels[0], measurementIndex);
            
            for (int recordingId = 0; recordingId < 120; recordingId++) /// Za potrebe testiranja
            {
                var row = CreateBaseRow(table, recordingId, timestamp);
                PopulateChannelData(row, channels, measurementIndex, type, harmonicType);
                table.Rows.Add(row);
            }
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Calculates the timestamp for a measurement index based on the channel's time series.
    /// </summary>
    /// <param name="startTime">The start time of the PQDIF file.</param>
    /// <param name="channel">The channel containing the time series.</param>
    /// <param name="index">The measurement index.</param>
    /// <returns>The calculated timestamp as a DateTime.</returns>
    private DateTime CalculateTimestamp(DateTime startTime, Channel channel, int index)
    {
        return startTime
            .AddSeconds((double)channel.TimeSeries.OriginalValues[index])
            .ToUniversalTime();
    }

    /// <summary>
    /// Creates a new DataRow with base columns populated.
    /// </summary>
    /// <param name="table">The DataTable to create the row for.</param>
    /// <param name="recordingId">The recording ID value.</param>
    /// <param name="timestamp">The timestamp value.</param>
    /// <returns>A new DataRow with base columns set.</returns>
    private DataRow CreateBaseRow(DataTable table, int recordingId, DateTime timestamp)
    {
        var row = table.NewRow();
        row["RecordingId"] = recordingId;
        row["TimeStamp"] = timestamp;
        return row;
    }

    /// <summary>
    /// Populates a DataRow with harmonic/interharmonic values for all relevant channels.
    /// </summary>
    /// <param name="row">The DataRow to populate.</param>
    /// <param name="channels">Array of channels to extract data from.</param>
    /// <param name="measurementIndex">The measurement index to use.</param>
    /// <param name="type">"Voltage" or "Current".</param>
    /// <param name="harmonicType">"Harmonic" or "Interharmonic".</param>
    private void PopulateChannelData(DataRow row, Channel[] channels, int measurementIndex, string type, string harmonicType)
    {
        foreach (var channel in channels)
        {
            if (!channel.ChannelName.Contains(harmonicType))
                continue;

            if (channel.QuantityMeasured.ToString() != type)
                continue;

            ProcessChannel(row, channel, measurementIndex);
        }
    }

    /// <summary>
    /// Determines if a channel is a harmonic or interharmonic channel by name.
    /// </summary>
    /// <param name="channelName">The channel name to check.</param>
    /// <returns>True if the channel is harmonic or interharmonic; otherwise, false.</returns>
    private bool IsHarmonicChannel(string channelName)
    {
        return channelName.Contains("Harmonic") || channelName.Contains("Interharmonic");
    }

    /// <summary>
    /// Processes a single channel and sets the appropriate harmonic value in the DataRow.
    /// </summary>
    /// <param name="row">The DataRow to update.</param>
    /// <param name="channel">The channel to process.</param>
    /// <param name="measurementIndex">The measurement index to use.</param>
    private void ProcessChannel(DataRow row, Channel channel, int measurementIndex)
    {
        var series = channel.ValueSeries[0];
        var phase = PhaseConverter.ConvertPhase(
            channel.Phase.ToString(), 
            channel.QuantityMeasured.ToString()
        );
        
        var harmonicIndex = ExtractHarmonicIndex(channel.ChannelName);
        if (!harmonicIndex.HasValue)
            return;

        var value = _randomizer.AdjustValue((double)series.OriginalValues[measurementIndex], 0.001);
        
        SetHarmonicValue(row, channel, phase, harmonicIndex.Value, value, measurementIndex);
    }

    /// <summary>
    /// Extracts the harmonic index from a channel name (e.g., "H3" returns 3).
    /// </summary>
    /// <param name="channelName">The channel name to parse.</param>
    /// <returns>The harmonic index if found; otherwise, null.</returns>
    private int? ExtractHarmonicIndex(string channelName)
    {
        var match = Regex.Match(channelName, @"H(\d+)");
        return match.Success ? int.Parse(match.Groups[1].Value) : null;
    }

    /// <summary>
    /// Sets the harmonic or interharmonic value in the DataRow, as base or percentage value.
    /// </summary>
    /// <param name="row">The DataRow to update.</param>
    /// <param name="channel">The channel being processed.</param>
    /// <param name="phase">The phase string for the column.</param>
    /// <param name="index">The harmonic/interharmonic index.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="measurementIndex">The measurement index.</param>
    private void SetHarmonicValue(DataRow row, Channel channel, string phase, int index, double value, int measurementIndex)
    {
        bool isInterharmonic = channel.ChannelName.Contains("Interharmonic");
        bool isCurrent = channel.ChannelName.Contains("Current");
        string suffix = isInterharmonic ? "IH" : "H";
        
        if (index == 1 && !isInterharmonic)
        {
            // Base value
            row[$"{phase}{suffix}1"] = (int)Math.Round(value*100);

        }
        else
        {
            // Percentage value
            if (FirstHarmonic[phase][measurementIndex] != DBNull.Value && (double)FirstHarmonic[phase][measurementIndex] != 0)
            {
                int targetIndex = isInterharmonic ? index + 1 : index; // TODO: namješteno za PQube - provjeriti kasnije
                row[$"{phase}{suffix}{targetIndex}"] = (int) Math.Round(value*100 / (double)FirstHarmonic[phase][measurementIndex] * 100);
            }
        }
    }
}