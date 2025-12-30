using System.Data;
using Gemstone.PQDIF.Logical;
using PQDIF_Manager;

/// <summary>
/// Populates a DataTable with event data extracted from a PQDIF file.
/// Handles waveform compression and mapping to event table columns.
/// </summary>
public class EventTablePopulator : IDataPopulator
{
    /// <summary>
    /// Populates the provided DataTable with event data from the PQDIF file.
    /// Compresses waveform data and maps channels to table columns.
    /// </summary>
    /// <param name="table">The DataTable to populate.</param>
    /// <param name="pqdifFile">The PQDIF file containing event data.</param>
    /// <returns>A completed Task.</returns>
    public Task PopulateAsync(DataTable table, PqdifFile pqdifFile)
    {
        Channel[] channels = pqdifFile.Channels;
        object[] timestampValues = channels[0].TimeSeries.OriginalValues.ToArray();
        Byte[]  timestamp = CompresssionHandler.CompressWaveform(timestampValues);
        for(int k = 0; k <= 1; k++)
        {
            for (int j = 0; j <= 1; j++)
            {
                DataRow row = table.NewRow();
                row["TypeId"] = j; 
                row["RecordingId"] = k;
                row["StartTime"] = pqdifFile.StartTime.AddMilliseconds((double)timestampValues[0]);
                row["EndTime"] = pqdifFile.StartTime.AddMilliseconds((double)timestampValues[timestampValues.Length - 1]);
                row["Timestamp"] = timestamp;
                for(int i = j; i < channels.Length; i+=2)
                {
                    Channel channel = channels[i];
                    
                    if (IsSkippable(channel))  continue;

                    Series series = channel.ValueSeries[0];
                    QuantityMeasured quantityMeasured = channel.QuantityMeasured;
                    string convertedPhase = PhaseConverter.ConvertPhase(channel.Phase.ToString(), quantityMeasured.ToString());
                    Random rand = new Random();


                    float[] values = series.OriginalValues
                        .Select(o => Convert.ToSingle(o))
                        .ToArray();

 
                    float[] modifiedValues = values
                        .Select(v =>
                        {
                            float percent = (float)(rand.NextDouble() * 0.05); 
                            return v * (1f + percent);
                        })
                        .ToArray();
                    Byte[]  compressedWaveform = CompresssionHandler.CompressWaveform(series.OriginalValues.ToArray());
                    row[convertedPhase] = compressedWaveform;
                }

                table.Rows.Add(row);
            }
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Determines if a channel should be skipped based on its name or phase.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns>True if the channel should be skipped; otherwise, false.</returns>
    private bool IsSkippable(Channel channel)
    {
        return channel.ChannelName.Contains("Power") || channel.ChannelName.Contains("Frequency") 
        || channel.Phase==Phase.NG || channel.Phase==Phase.Net || channel.Phase.ToString().Contains("General");
    }
}