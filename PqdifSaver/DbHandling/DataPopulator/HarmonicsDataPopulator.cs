using System.Data;
using System.Text.RegularExpressions;
using PQDIF_Manager;

public class HarmonicsDataPopulator : IDataPopulator
{
    private readonly ValueRandomizer _randomizer = new();

    public async Task PopulateAsync(DataTable table, PqdifFile pqdifFile)
    {
        var channels = pqdifFile.Channels;
        int totalMeasurements = channels[0].TimeSeries.SampleCount;
        DateTime startTime = pqdifFile.StartTime;
        string type = (table.Columns.Contains("U1H0") || table.Columns.Contains("U1IH0") )  ? "Voltage" : "Current";
        string harmonicType = (table.Columns.Contains("U1H0") || table.Columns.Contains("I1H0") )  ? "Harmonic" : "Interharmonic";


        for (int measurementIndex = 0; measurementIndex < totalMeasurements; measurementIndex++)
        {
            var timestamp = CalculateTimestamp(startTime, channels[0], measurementIndex);
            
            for (int recordingId = 0; recordingId < 1; recordingId++) /// Za potrebe testiranja
            {
                var row = CreateBaseRow(table, recordingId, timestamp);
                PopulateChannelData(row, channels, measurementIndex, type, harmonicType);
                table.Rows.Add(row);
            }
        }

        await Task.CompletedTask;
    }

    private DateTime CalculateTimestamp(DateTime startTime, Channel channel, int index)
    {
        return startTime
            .AddSeconds((double)channel.TimeSeries.OriginalValues[index])
            .ToUniversalTime();
    }

    private DataRow CreateBaseRow(DataTable table, int recordingId, DateTime timestamp)
    {
        var row = table.NewRow();
        row["RecordingId"] = recordingId;
        row["TimeStamp"] = timestamp;
        return row;
    }

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

    private bool IsHarmonicChannel(string channelName)
    {
        return channelName.Contains("Harmonic") || channelName.Contains("Interharmonic");
    }

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
        
        SetHarmonicValue(row, channel, phase, harmonicIndex.Value, value);
    }

    private int? ExtractHarmonicIndex(string channelName)
    {
        var match = Regex.Match(channelName, @"H(\d+)");
        return match.Success ? int.Parse(match.Groups[1].Value) : null;
    }

    private void SetHarmonicValue(DataRow row, Channel channel, string phase, int index, double value)
    {
        bool isInterharmonic = channel.ChannelName.Contains("Interharmonic");
        string suffix = isInterharmonic ? "IH" : "H";
        
        if (index == (isInterharmonic ? 0 : 1))
        {
            // Base value
            row[$"{phase}{suffix}{(isInterharmonic ? index : 0)}"] = (int)Math.Round(value*100);
        }
        else
        {
            // Percentage value
            string baseColumn = $"{phase}{suffix}0";
            if (row[baseColumn] != DBNull.Value && (int)row[baseColumn] != 0)
            {
                int targetIndex = isInterharmonic ? index : index - 1;
                row[$"{phase}{suffix}{targetIndex}"] = (int) Math.Round(value*100 / (int)row[baseColumn] * 100);
            }
        }
    }
}