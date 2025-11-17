using Microsoft.Data.SqlClient;

public class SqlServerSeriesInfoRepository : ISeriesInfoRepository
{
    private readonly string _connectionString;

    public SqlServerSeriesInfoRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Task<int> GetSeriesIdAsync(string channelName, string quantityMeasured, string phase, string seriesValueType)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("SELECT SeriesId FROM SeriesInfo WHERE ChannelName = @ChannelName AND QuantityMeasured = @QuantityMeasured AND Phase = @Phase AND SeriesValueType = @SeriesValueType", connection);
            command.Parameters.AddWithValue("@ChannelName", channelName);
            command.Parameters.AddWithValue("@QuantityMeasured", quantityMeasured);
            command.Parameters.AddWithValue("@Phase", phase);
            command.Parameters.AddWithValue("@SeriesValueType", seriesValueType);

            var result = command.ExecuteScalar();
            if (result != null)
            {
                return Task.FromResult(Convert.ToInt32(result));
            }

            return Task.FromResult(0);
        }
    }

    public async Task SaveSeriesInfoAsync(SeriesInfo seriesInfo)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var command = new SqlCommand("INSERT INTO SeriesInfo (ChannelName, QuantityMeasured, Phase, SeriesValueType) VALUES (@ChannelName, @QuantityMeasured, @Phase, @SeriesValueType); SELECT SCOPE_IDENTITY();", connection);
            command.Parameters.AddWithValue("@ChannelName", seriesInfo.ChannelName);
            command.Parameters.AddWithValue("@QuantityMeasured", seriesInfo.QuantityMeasured);
            command.Parameters.AddWithValue("@Phase", seriesInfo.Phase);
            command.Parameters.AddWithValue("@SeriesValueType", seriesInfo.SeriesValueType);

            var result = await command.ExecuteScalarAsync();
            seriesInfo.SeriesId = Convert.ToInt32(result);
        }
    }
}