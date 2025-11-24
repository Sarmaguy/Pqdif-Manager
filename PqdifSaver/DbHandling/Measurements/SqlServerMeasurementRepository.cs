using System.Data;
using Gemstone.PQDIF.Logical;
using Microsoft.Data.SqlClient;
using PQDIF_Manager;
using System.Text.RegularExpressions;
public class SqlServerMeasurementRepository : IMeasurementRepository
{
    private readonly string _connectionString;

    public SqlServerMeasurementRepository()
    {
        _connectionString = ConfigBuilder.Instance.ConnectionString;
    }

    private async Task BulkInsertVoltageHarmonicsAsync(DataTable table)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using SqlBulkCopy bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = "VoltageHarmonics"
        };

        foreach (DataColumn col in table.Columns)  bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
        await bulkCopy.WriteToServerAsync(table);
    }
    private async Task BulkInsertCurrentHarmonicsAsync(DataTable table)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using SqlBulkCopy bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = "CurrentHarmonics"
        };
        foreach (DataColumn col in table.Columns) bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
        await bulkCopy.WriteToServerAsync(table); 
    }
    private async Task BulkInsertVoltageInterharmonicsAsync(DataTable table)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using SqlBulkCopy bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = "VoltageInterharmonics"
        };
        foreach (DataColumn col in table.Columns) bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
        await bulkCopy.WriteToServerAsync(table);
    }
    private async Task BulkInsertCurrentInterharmonicsAsync(DataTable table)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using SqlBulkCopy bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = "CurrentInterharmonics"
        };
        foreach (DataColumn col in table.Columns) bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
        await bulkCopy.WriteToServerAsync(table);
    }

    public async Task BulkInsertHarmonicsAsync(PqdifFile pqdifFile)
    {
        Channel[] channels = pqdifFile.Channels;
        DataTable VoltageHarmonicsTable = new DataTable();
        DataTable VoltageInterharmonicsTable = new DataTable();
        DataTable CurrentHarmonicsTable = new DataTable();
        DataTable CurrentInterharmonicsTable = new DataTable();

        VoltageHarmonicsTable.Columns.Add("RecordingId", typeof(string));
        VoltageHarmonicsTable.Columns.Add("TimeStamp", typeof(DateTime));
        VoltageInterharmonicsTable.Columns.Add("RecordingId", typeof(string));
        VoltageInterharmonicsTable.Columns.Add("TimeStamp", typeof(DateTime));
        CurrentHarmonicsTable.Columns.Add("RecordingId", typeof(string));
        CurrentHarmonicsTable.Columns.Add("TimeStamp", typeof(DateTime));
        CurrentInterharmonicsTable.Columns.Add("RecordingId", typeof(string));
        CurrentInterharmonicsTable.Columns.Add("TimeStamp", typeof(DateTime));

        for (int i = 0; i < 63; i++)
        {
            for (int j = 1; j <= 3; j++)
            {
                /* U1H0, U1H1, U1H2 …U1H63
                    U2H0, U2H1, U2H2 …U2H63
                    U3H0, U3H1, U3H2 …U3H63 */
                VoltageHarmonicsTable.Columns.Add($"U{j}H{i}", typeof(double));

                /* U12H0, U12H1, U12H2 …U12H63
                    U23H0, U23H1, U23H2 …U23H63
                    U31H0, U31H1, U31H2 …U31H63 */
                VoltageHarmonicsTable.Columns.Add($"U{j}{(j % 3) + 1}H{i}", typeof(double));

                /* I1H0, I1H1, I1H2 …I1H63
                    I2H0, I2H1, I2H2 …I2H63
                    I3H0, I3H1, I3H2 …I3H63 */
                CurrentHarmonicsTable.Columns.Add($"I{j}H{i}", typeof(double));
            }
            //UNH0, UNH1, UNH2 …UNH63
            VoltageHarmonicsTable.Columns.Add($"UNH{i}", typeof(double));

            //INH0, INH1, INH2 …INH63
            CurrentHarmonicsTable.Columns.Add($"INH{i}", typeof(double));

            //interharmonics
            if (i <= 49 && i >= 0)
            {
                for (int j = 1; j <= 3; j++)
                {
                    /*  U1IH1, U1IH2 …U1IH49
                        U2IH1, U2IH2 …U2IH49
                        U3IH1, U3IH2 …U3IH49 */
                    VoltageInterharmonicsTable.Columns.Add($"U{j}IH{i}", typeof(double));

                    /*  U12IH1, U12IH2 …U12IH49
                        U23IH1, U23IH2 …U23IH49
                        U31IH1, U31IH2 …U31IH49 */
                    VoltageInterharmonicsTable.Columns.Add($"U{j}{(j % 3) + 1}IH{i}", typeof(double));

                    /*  I1IH1, I1IH2 …I1IH49
                        I2IH1, I2IH2 …I2IH49
                        I3IH1, I3IH2 …I3IH49 */
                    CurrentInterharmonicsTable.Columns.Add($"I{j}IH{i}", typeof(double));
                }
                //UNIH1, UNIH2 …UNIH49
                VoltageInterharmonicsTable.Columns.Add($"UNIH{i}", typeof(double));

                //INIH0, INIH1, INIH2 …INIH49
                CurrentInterharmonicsTable.Columns.Add($"INIH{i}", typeof(double));
            }
        }
        
        int totalMeasurements = channels[0].TimeSeries.SampleCount;
        DateTime sartTime = pqdifFile.StartTime;
        for (int i = 0; i < totalMeasurements; i++)
        {
            DateTime timeStamp = sartTime.AddSeconds((double)channels[0].TimeSeries.OriginalValues[i]).ToUniversalTime();
            DataRow voltageHarmonicsRow = VoltageHarmonicsTable.NewRow();
            DataRow voltageInterharmonicsRow = VoltageInterharmonicsTable.NewRow();
            DataRow currentHarmonicsRow = CurrentHarmonicsTable.NewRow();
            DataRow currentInterharmonicsRow = CurrentInterharmonicsTable.NewRow();

            voltageHarmonicsRow["RecordingId"] = pqdifFile.RecordingId;
            voltageHarmonicsRow["TimeStamp"] = timeStamp;
            voltageInterharmonicsRow["RecordingId"] = pqdifFile.RecordingId;
            voltageInterharmonicsRow["TimeStamp"] = timeStamp;
            currentHarmonicsRow["RecordingId"] = pqdifFile.RecordingId;
            currentHarmonicsRow["TimeStamp"] = timeStamp;
            currentInterharmonicsRow["RecordingId"] = pqdifFile.RecordingId;
            currentInterharmonicsRow["TimeStamp"] = timeStamp;

            foreach (var channel in channels)
            {
                string ChannelName = channel.ChannelName;

                if (!ChannelName.Contains("Interharmonic") && !ChannelName.Contains("Harmonic")) continue;

                Series series = channel.ValueSeries[0];
                QuantityMeasured quantityMeasured = channel.QuantityMeasured;
                string convertedPhase = PhaseConverter.ConvertPhase(channel.Phase.ToString(), quantityMeasured.ToString());
                int index;
                var match = Regex.Match(ChannelName, @"H(\d+)");
 
                if (match.Success) index = int.Parse(match.Groups[1].Value);
                else continue;
                

                if (ChannelName.Contains("Interharmonic"))
                {
                    if (quantityMeasured == QuantityMeasured.Voltage)
                        voltageInterharmonicsRow[$"{convertedPhase}IH{index}"] = series.OriginalValues[i];

                    else if (quantityMeasured == QuantityMeasured.Current)
                        currentInterharmonicsRow[$"{convertedPhase}IH{index}"] = series.OriginalValues[i];
                }
                else if (ChannelName.Contains("Harmonic"))
                {
                    if (quantityMeasured == QuantityMeasured.Voltage)
                        voltageHarmonicsRow[$"{convertedPhase}H{index-1}"] = series.OriginalValues[i];

                    else if (quantityMeasured == QuantityMeasured.Current)
                        currentHarmonicsRow[$"{convertedPhase}H{index-1}"] = series.OriginalValues[i];
                    
                }
            }

            VoltageHarmonicsTable.Rows.Add(voltageHarmonicsRow);
            VoltageInterharmonicsTable.Rows.Add(voltageInterharmonicsRow);
            CurrentHarmonicsTable.Rows.Add(currentHarmonicsRow);
            CurrentInterharmonicsTable.Rows.Add(currentInterharmonicsRow);
        }
        await BulkInsertVoltageHarmonicsAsync(VoltageHarmonicsTable);
        await BulkInsertVoltageInterharmonicsAsync(VoltageInterharmonicsTable);
        await BulkInsertCurrentHarmonicsAsync(CurrentHarmonicsTable);
        await BulkInsertCurrentInterharmonicsAsync(CurrentInterharmonicsTable);
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