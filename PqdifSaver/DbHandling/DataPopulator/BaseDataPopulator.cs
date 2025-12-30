using System.Data;
using Gemstone.PQDIF.Logical;
using PQDIF_Manager;

/// <summary>
/// Provides an implementation for populating a DataTable for Base table with measurement data from a PQDIF file.
/// Handles channel preprocessing, timestamp calculation, and value mapping for measurement series.
/// </summary>
public class BaseDataPopulator : IDataPopulator
{
    private readonly ValueRandomizer _randomizer = new();

    /// <summary>
    /// Populates the provided DataTable with measurement data extracted from the PQDIF file.
    /// </summary>
    /// <param name="table">The DataTable to populate.</param>
    /// <param name="pqdifFile">The PQDIF file containing measurement data.</param>
    public async Task PopulateAsync(DataTable table, PqdifFile pqdifFile)
    {
        var channels = PreprocessChannels(pqdifFile.Channels);
        int totalMeasurements = channels[0].ValueSeries[0].SampleCount;

        for (int i = 0; i < totalMeasurements; i++)
        {
            for (int recordingId = 0; recordingId < 120; recordingId++) // Za potrebe testiranja
            {
                var row = table.NewRow();
                row["RecordingId"] = recordingId;
                row["Time"] = CalculateTimestamp(pqdifFile.StartTime, channels[0], i);
                PopulateSeriesData(row, channels, i);
                table.Rows.Add(row);
            }
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Normalizes phases for all channels and returns the processed array.
    /// </summary>
    /// <param name="channels">Array of channels to preprocess.</param>
    /// <returns>Processed array of channels.</returns>
    private Channel[] PreprocessChannels(Channel[] channels)
    {
        foreach (var channel in channels)
        {
            NormalizePhase(channel);
        }
        return channels;
    }

    /// <summary>
    /// Sets the Phase property of a channel based on its name. THIS IS A WORKAROUND FOR BAD DATA!!!
    /// </summary>
    /// <param name="channel">The channel to normalize.</param>
    private void NormalizePhase(Channel channel)
    {
        if (channel.ChannelName.Contains("L1-N")) channel.Phase = Phase.AN;
        else if (channel.ChannelName.Contains("L2-N")) channel.Phase = Phase.BN;
        else if (channel.ChannelName.Contains("L3-N")) channel.Phase = Phase.CN;
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
    /// Populates a DataRow with measurement values from all relevant channels and series.
    /// </summary>
    /// <param name="row">The DataRow to populate.</param>
    /// <param name="channels">Array of channels to extract data from.</param>
    /// <param name="measurementIndex">The measurement index to use.</param>
    private void PopulateSeriesData(DataRow row, Channel[] channels, int measurementIndex)
    {
        foreach (var channel in channels)
        {
            if (channel.Phase.ToString().Contains("LineTo"))
                continue;

            foreach (var series in channel.ValueSeries)
            {
                if (ShouldSkipSeries(series))
                    continue;

                var columnName = GetColumnName(channel, series);
                if (columnName == null || columnName.Contains("Hx"))
                    continue;

                SetSeriesValue(row, series, columnName, measurementIndex);
            }
        }
    }

    /// <summary>
    /// Determines if a series should be skipped based on its characteristic.
    /// </summary>
    /// <param name="series">The series to check.</param>
    /// <returns>True if the series should be skipped; otherwise, false.</returns>
    private bool ShouldSkipSeries(Series series)
    {
        return series.QuantityCharacteristic != null && 
               series.QuantityCharacteristic.StartsWith("Spectra by");
    }

    /// <summary>
    /// Gets the column name for a given channel and series, using MeasurementTypes mapping.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="series">The series.</param>
    /// <returns>The column name, or null if not applicable.</returns>
    private string? GetColumnName(Channel channel, Series series)
    {
        var characteristic = series.QuantityCharacteristic;
        
        if (characteristic != null && 
            (characteristic.Contains("Negative sequence") || characteristic.Contains("Zero sequence")))
        {
            characteristic = $"{channel.QuantityMeasured} {characteristic}";
        }

        return MeasurementTypes.GetTableColumn(
            channel.Phase,
            series.SeriesValueType,
            series.QuantityUnits.ToString(),
            characteristic
        );
    }

    /// <summary>
    /// Sets the value for a specific column in the DataRow based on the series data.
    /// <param name="row">The DataRow to update.</param>
    /// <param name="series">The series providing the value.</param>
    /// <param name="columnName">The column name to set.</param>
    /// <param name="index">The measurement index.</param>
    private void SetSeriesValue(DataRow row, Series series, string columnName, int index)
    {
        if (series.OriginalValues.Count <= index)
        {
            row[columnName] = DBNull.Value;
            return;
        }

        double value = (double)series.OriginalValues[index];
        
        if (columnName.Contains("PF"))
        {
            row[columnName] = (int)(_randomizer.AdjustValue(value, 0.01) * 1000);
        }
        else
        {
            row[columnName] = Math.Round(_randomizer.AdjustValue(value, 0.01)*100);
        }
    }
}