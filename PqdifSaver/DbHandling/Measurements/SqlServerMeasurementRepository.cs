using System.Data;
using Microsoft.Data.SqlClient;
using PQDIF_Manager;
public class SqlServerMeasurementRepository : IMeasurementRepository
{
    private readonly string _connectionString;

    public SqlServerMeasurementRepository()
    {
        _connectionString = ConfigBuilder.Instance.ConnectionString;
    }

    public async Task BulkInsertBaseAsync(PqdifFile pqdifFile)
    {
        Channel[] channels = pqdifFile.Channels;
        using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using SqlBulkCopy bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = "base"
        };
        DataTable table = new DataTable();
        table.Columns.Add("RecordingId", typeof(string));
        table.Columns.Add("Time", typeof(DateTime));

        foreach (var channel in channels)
        {
            foreach (var series in channel.ValueSeries)
            {
                if (series.QuantityCharacteristic != null && series.QuantityCharacteristic.StartsWith("Spectra by")) continue;

                if ((series.QuantityCharacteristic != null) && (series.QuantityCharacteristic.Contains("Negative sequence component unbalance (%)") || 
                    series.QuantityCharacteristic.Contains("Zero sequence component unbalance (%)")))
                    series.QuantityCharacteristic = $"{channel.QuantityMeasured} {series.QuantityCharacteristic}";

                string? ColumnName = MeasurementTypes.GetTableColumn(channel.Phase, series.SeriesValueType, series.QuantityUnits.ToString(), series.QuantityCharacteristic);
                //Console.WriteLine($"Processing channel: {channel.ChannelName}");
                //Console.WriteLine($"Mapping {channel.Phase} - {series.SeriesValueType} - {series.QuantityUnits} - {series.QuantityCharacteristic} to column {ColumnName}");

                if (ColumnName != null && !ColumnName.Contains("Hx")) table.Columns.Add(ColumnName, typeof(double));

            }
        }

        foreach (DataColumn col in table.Columns) bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);



        int totalMeasurements = channels[0].ValueSeries[0].SampleCount;
        for (int i = 0; i < totalMeasurements; i++)
        {
            DateTime timeStamp = pqdifFile.StartTime;
            DataRow row = table.NewRow();
            row["RecordingId"] = pqdifFile.RecordingId;
            row["Time"] = timeStamp.AddSeconds((double)channels[0].TimeSeries.OriginalValues[i]).ToUniversalTime();

            foreach (var channel in channels)
            {
                foreach (var series in channel.ValueSeries)
                {
                    if (series.QuantityCharacteristic != null && series.QuantityCharacteristic.StartsWith("Spectra by")) continue;
                    string? ColumnName = MeasurementTypes.GetTableColumn(channel.Phase, series.SeriesValueType, series.QuantityUnits.ToString(), series.QuantityCharacteristic);

                    if (ColumnName != null)
                    {
                        if (series.OriginalValues.Count <= i)
                        {
                            row[ColumnName] = DBNull.Value;
                            continue;
                        }
                        row[ColumnName] = series.OriginalValues[i];
                    }
                }
            }

            table.Rows.Add(row);
        }
        await bulkCopy.WriteToServerAsync(table);
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