using System.Data;
using System.Text.RegularExpressions;
using PQDIF_Manager;

public class HarmonicsDataPopulator : IDataPopulator
{
    private readonly ValueRandomizer _randomizer = new();
    private static Dictionary<string, IList<object>> FirstHarmonic =  new Dictionary<string, IList<object>>();

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
        
        SetHarmonicValue(row, channel, phase, harmonicIndex.Value, value, measurementIndex);
    }

    private int? ExtractHarmonicIndex(string channelName)
    {
        var match = Regex.Match(channelName, @"H(\d+)");
        return match.Success ? int.Parse(match.Groups[1].Value) : null;
    }

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