using System.Data;
using Microsoft.Data.SqlClient;
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
        table.Columns.Add("ChannelId", typeof(int));

        foreach (var measurement in measurements)
        {
            table.Rows.Add(measurement.RecordingId, measurement.timestamp, measurement.Value, measurement.ChannelId);
        }

        await bulkCopy.WriteToServerAsync(table);
    }
}