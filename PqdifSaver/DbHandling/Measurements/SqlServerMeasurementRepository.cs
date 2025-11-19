using System.Data;
using Microsoft.Data.SqlClient;
using PQDIF_Manager;
public class SqlServerMeasurementRepository : IMeasurementRepository
{
    private readonly string _connectionString;

    public SqlServerMeasurementRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task BulkInsertAsync(IEnumerable<Measurement> measurements)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using SqlBulkCopy bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = "Measurements"
        };

        DataTable table = new DataTable();
        table.Columns.Add("RecordingId", typeof(string));
        table.Columns.Add("timestamp", typeof(DateTime));
        table.Columns.Add("Value", typeof(double));
        table.Columns.Add("SeriesId", typeof(int));

        foreach (var measurement in measurements)
        {
            table.Rows.Add(measurement.RecordingId, measurement.timestamp, measurement.Value, measurement.SeriesId);
        }

        await bulkCopy.WriteToServerAsync(table);
    }

    public async Task BulkInsertBigAsync(PqdifFile pqdifFile)
    {
        Channel[] channels = pqdifFile.Channels;

        using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using SqlBulkCopy bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = "MeasurementsBig"
        };

        DataTable table = new DataTable();
        table.Columns.Add("RecordingId", typeof(string));
        table.Columns.Add("timestamp", typeof(DateTime));
        foreach (var channel in channels)
        {
            foreach (var series in channel.ValueSeries)
            {
                        string ColumnName = channel.ChannelName.Replace(" ", "_") + "_" + series.SeriesValueType;
                        table.Columns.Add(ColumnName, typeof(double));
            }
        }

        int totalMeasurements = channels[0].ValueSeries[0].SampleCount;
        for (int i = 0; i < totalMeasurements; i++)
        {
            DateTime timeStamp = pqdifFile.StartTime;
            DataRow row = table.NewRow();
            row["RecordingId"] = pqdifFile.RecordingId;
            row["timestamp"] = timeStamp.AddSeconds((double)channels[0].TimeSeries.OriginalValues[i]).ToUniversalTime();

            foreach (var channel in channels)
            {
                foreach (var series in channel.ValueSeries)
                {
                    string ColumnName = channel.ChannelName.Replace(" ", "_") + "_" + series.SeriesValueType;

                    if(series.OriginalValues.Count <= i)
                    {
                        row[ColumnName] = DBNull.Value;
                        continue;
                    }
                    row[ColumnName] = series.OriginalValues[i];
                }
            }

            table.Rows.Add(row);
        }

        await bulkCopy.WriteToServerAsync(table);
        
    }
}