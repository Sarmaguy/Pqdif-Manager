using System.Data;
using Gemstone.PQDIF.Logical;
using PQDIF_Manager;

public class BaseDataPopulator : IDataPopulator
{
    private readonly ValueRandomizer _randomizer = new();

    public async Task PopulateAsync(DataTable table, PqdifFile pqdifFile)
    {
        var channels = PreprocessChannels(pqdifFile.Channels);
        int totalMeasurements = channels[0].ValueSeries[0].SampleCount;

        for (int i = 0; i < totalMeasurements; i++)
        {
            for (int recordingId = 0; recordingId < 1; recordingId++) // Za potrebe testiranja
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

    private Channel[] PreprocessChannels(Channel[] channels)
    {
        foreach (var channel in channels)
        {
            NormalizePhase(channel);
        }
        return channels;
    }

    private void NormalizePhase(Channel channel)
    {
        if (channel.ChannelName.Contains("L1-N")) channel.Phase = Phase.AN;
        else if (channel.ChannelName.Contains("L2-N")) channel.Phase = Phase.BN;
        else if (channel.ChannelName.Contains("L3-N")) channel.Phase = Phase.CN;
    }

    private DateTime CalculateTimestamp(DateTime startTime, Channel channel, int index)
    {
        return startTime
            .AddSeconds((double)channel.TimeSeries.OriginalValues[index])
            .ToUniversalTime();
    }

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

    private bool ShouldSkipSeries(Series series)
    {
        return series.QuantityCharacteristic != null && 
               series.QuantityCharacteristic.StartsWith("Spectra by");
    }

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