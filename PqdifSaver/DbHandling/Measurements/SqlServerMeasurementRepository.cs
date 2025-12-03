using System.Data;
using Gemstone.PQDIF.Logical;
using Microsoft.Data.SqlClient;
using PQDIF_Manager;
using System.Text.RegularExpressions;
using System.Numerics;
public class SqlServerMeasurementRepository : IMeasurementRepository
{
    private readonly string _connectionString;
    public static int size = 0;
    public static BigInteger n = 0;

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
            DestinationTableName = "VoltageHarmonicsNew"
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
            DestinationTableName = "CurrentHarmonicsNew"
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
            DestinationTableName = "VoltageInterharmonicsNew"
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
            DestinationTableName = "CurrentInterharmonicsNew"
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
            double UH1 = 0;
            double IH1= 0;


            for (int j = 0; j < 120; j++)
            {
                
                DataRow voltageHarmonicsRow = VoltageHarmonicsTable.NewRow();
                DataRow voltageInterharmonicsRow = VoltageInterharmonicsTable.NewRow();
                DataRow currentHarmonicsRow = CurrentHarmonicsTable.NewRow();
                DataRow currentInterharmonicsRow = CurrentInterharmonicsTable.NewRow();

                voltageHarmonicsRow["RecordingId"] = j;
                voltageHarmonicsRow["TimeStamp"] = timeStamp;
                voltageInterharmonicsRow["RecordingId"] = j;
                voltageInterharmonicsRow["TimeStamp"] = timeStamp;
                currentHarmonicsRow["RecordingId"] = j;
                currentHarmonicsRow["TimeStamp"] = timeStamp;
                currentInterharmonicsRow["RecordingId"] = j;
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

                    var Random = new Random();
                    int sign = Random.Next(0, 2) == 0 ? 1 : -1;
                    double ValueAdjustmentFactor = 1 + sign * Random.NextDouble() / 1000;
                    double value = (double)series.OriginalValues[i] * ValueAdjustmentFactor;



                    

                    if (ChannelName.Contains("Interharmonic"))
                    {
                        if(quantityMeasured == QuantityMeasured.Voltage && index == 0) voltageInterharmonicsRow[$"{convertedPhase}IH{index}"] = value;

                        else if (quantityMeasured == QuantityMeasured.Current && index == 0) currentInterharmonicsRow[$"{convertedPhase}IH{index}"] = value;

                        else if (quantityMeasured == QuantityMeasured.Voltage)
                            voltageInterharmonicsRow[$"{convertedPhase}IH{index}"] = Math.Round(value/(double)voltageInterharmonicsRow[$"{convertedPhase}IH{0}"]*100);

                        else if (quantityMeasured == QuantityMeasured.Current)
                            currentInterharmonicsRow[$"{convertedPhase}IH{index}"] = Math.Round(value/(double)currentInterharmonicsRow[$"{convertedPhase}IH{0}"]*100);
                    }
                    else if (ChannelName.Contains("Harmonic"))
                    {
                        if (quantityMeasured == QuantityMeasured.Voltage && index == 1) voltageHarmonicsRow[$"{convertedPhase}H{0}"] = value;

                        else if (quantityMeasured == QuantityMeasured.Current && index == 1) currentHarmonicsRow[$"{convertedPhase}H{0}"] = value;


                        else if (quantityMeasured == QuantityMeasured.Voltage)
                            voltageHarmonicsRow[$"{convertedPhase}H{index-1}"] = Math.Round(value/(double)voltageHarmonicsRow[$"{convertedPhase}H{0}"]*100);

                        else if (quantityMeasured == QuantityMeasured.Current)
                            currentHarmonicsRow[$"{convertedPhase}H{index-1}"] = Math.Round(value/(double)currentHarmonicsRow[$"{convertedPhase}H{0}"]*100);
                        
                    }
                }

                VoltageHarmonicsTable.Rows.Add(voltageHarmonicsRow);
                VoltageInterharmonicsTable.Rows.Add(voltageInterharmonicsRow);
                CurrentHarmonicsTable.Rows.Add(currentHarmonicsRow);
                CurrentInterharmonicsTable.Rows.Add(currentInterharmonicsRow);
            }
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
            DestinationTableName = "trend"
        };
        DataTable table = new DataTable();
        table.Columns.Add("RecordingId", typeof(string));
        table.Columns.Add("Time", typeof(DateTime));

        foreach (var channel in channels)
        {
            if(channel.Phase.ToString().Contains("LineTo")) continue;
            if(channel.ChannelName.Contains("L1-N")) channel.Phase = Phase.AN;
            else if(channel.ChannelName.Contains("L2-N")) channel.Phase = Phase.BN;
            else if(channel.ChannelName.Contains("L3-N")) channel.Phase = Phase.CN;

            foreach (var series in channel.ValueSeries)
            {
                if (series.QuantityCharacteristic != null && series.QuantityCharacteristic.StartsWith("Spectra by")) continue;

                

                

                if ((series.QuantityCharacteristic != null) && (series.QuantityCharacteristic.Contains("Negative sequence component unbalance (%)") || 
                    series.QuantityCharacteristic.Contains("Zero sequence component unbalance (%)")))
                    series.QuantityCharacteristic = $"{channel.QuantityMeasured} {series.QuantityCharacteristic}";

                string? ColumnName = MeasurementTypes.GetTableColumn(channel.Phase, series.SeriesValueType, series.QuantityUnits.ToString(), series.QuantityCharacteristic);
/*                 Console.WriteLine($"Processing channel: {channel.ChannelName}");
                Console.WriteLine($"Mapping {channel.Phase} - {series.SeriesValueType} - {series.QuantityUnits} - {series.QuantityCharacteristic} to column {ColumnName}");
 */
                if (ColumnName != null && !ColumnName.Contains("Hx")) table.Columns.Add(ColumnName, typeof(double));

            }
        }

        foreach (DataColumn col in table.Columns) bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);



        int totalMeasurements = channels[0].ValueSeries[0].SampleCount;
        for (int i = 0; i < totalMeasurements; i++)
        {
            DateTime timeStamp = pqdifFile.StartTime;

            for (int j = 0; j < 120; j++)
            {

                DataRow row = table.NewRow();
                row["RecordingId"] = j;
                row["Time"] = timeStamp.AddSeconds((double)channels[0].TimeSeries.OriginalValues[i]).ToUniversalTime();
                var rand = new Random();

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
                            int sign = rand.Next(0, 2) == 0 ? 1 : -1;

                            if(ColumnName.Contains("PF")) {
                                row[ColumnName] = (int)((double) series.OriginalValues[i] * (1 + sign * rand.NextDouble() / 100) * 1000);
                                continue;
                            }

                            row[ColumnName] = ((double) series.OriginalValues[i] * (1 + sign * rand.NextDouble() / 100));

                        }
                    }
                }

                table.Rows.Add(row);
            }
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
        string temp = measurements.First().RecordingId +  Random.Shared.Next(100000000, 999999999).ToString();

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

    public async Task BulkInsertAsyncNew(IEnumerable<Measurement> measurements)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using SqlBulkCopy bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = "FreqInt"
        };
        DataTable table = new DataTable();
        table.Columns.Add("RecordingId", typeof(string));
        table.Columns.Add("Timestamp", typeof(DateTime));
        table.Columns.Add("Value", typeof(int));

        foreach (var measurement in measurements) for (int i = 0; i < 120; i++) table.Rows.Add(i, measurement.timestamp, (int)((measurement.Value +  Random.Shared.NextDouble()/100)*10000));


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

    public async Task BulkInsertFreq60(PqdifFile pqdifFile)
    {
        string temp = pqdifFile.RecordingId;
        using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using SqlBulkCopy bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = "Frequency60Columnstore"
        };
        DataTable table = new DataTable();
        table.Columns.Add("RecordingId", typeof(int));
        table.Columns.Add("TimeStamp", typeof(DateTime));

        for (int i = 1; i <= 60; i++) table.Columns.Add($"Freq{i}", typeof(double));

        foreach (DataColumn col in table.Columns) bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);

        int totalMeasurements = pqdifFile.Channels[0].TimeSeries.SampleCount / 60;
        DateTime startTime = pqdifFile.StartTime.AddYears(1);

        for (int i = 0; i < totalMeasurements; i++)
        {
            DateTime timeStamp = startTime.AddSeconds((double)pqdifFile.Channels[0].TimeSeries.OriginalValues[i * 60]).ToUniversalTime();

            for (int k = 0; k < 120; k++){
                
                DataRow row = table.NewRow();
                row["RecordingId"] = k;
                row["TimeStamp"] = timeStamp;

                for (int j = 0; j < 60; j++)
                {
                    Series series = pqdifFile.Channels[0].ValueSeries[0];
                    row[$"Freq{j + 1}"] = (((double)series.OriginalValues[i * 60 + j] + Random.Shared.NextDouble()/100));
                }

                table.Rows.Add(row);
            }
        }

        await bulkCopy.WriteToServerAsync(table);
    }

    public async Task BulkInsertFreq720(PqdifFile pqdifFile)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using SqlBulkCopy bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = "Frequency720Int"
        };  
        DataTable table = new DataTable();
        table.Columns.Add("RecordingId", typeof(int));
        table.Columns.Add("TimeStamp", typeof(DateTime));

        for (int i = 1; i <= 720; i++) table.Columns.Add($"Freq{i}", typeof(int));

        foreach (DataColumn col in table.Columns) bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);

        int totalMeasurements = pqdifFile.Channels[0].TimeSeries.SampleCount / 720;
        DateTime startTime = pqdifFile.StartTime;

        for (int i = 0; i < totalMeasurements; i++)
        {
            DateTime timeStamp = startTime.AddSeconds((double)pqdifFile.Channels[0].TimeSeries.OriginalValues[i * 720]).ToUniversalTime();

            for (int k = 0; k < 120; k++)
            {

                DataRow row = table.NewRow();
                row["RecordingId"] = k;
                row["TimeStamp"] = timeStamp;

                for (int j = 0; j < 720; j++)
                {
                    Series series = pqdifFile.Channels[0].ValueSeries[0];
                    row[$"Freq{j + 1}"] =(int) (((double) series.OriginalValues[i * 720 + j] + Random.Shared.NextDouble()/100)*10000);
                }

                table.Rows.Add(row);
            }
        }

        await bulkCopy.WriteToServerAsync(table);
    }


    public async Task BulkInsertFreq60Percentage(PqdifFile pqdifFile)
    {
        string temp = pqdifFile.RecordingId;
        using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using SqlBulkCopy bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = "Frequency60Percentage"
        };
        DataTable table = new DataTable();
        table.Columns.Add("RecordingId", typeof(int));
        table.Columns.Add("TimeStamp", typeof(DateTime));

        for (int i = 1; i <= 60; i++) table.Columns.Add($"Freq{i}", typeof(double));

        foreach (DataColumn col in table.Columns) bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);

        int totalMeasurements = pqdifFile.Channels[0].TimeSeries.SampleCount / 60;
        DateTime startTime = pqdifFile.StartTime.AddYears(1);

        for (int i = 0; i < totalMeasurements; i++)
        {
            DateTime timeStamp = startTime.AddSeconds((double)pqdifFile.Channels[0].TimeSeries.OriginalValues[i * 60]).ToUniversalTime();

            for (int k = 0; k < 120; k++){
                
                DataRow row = table.NewRow();
                row["RecordingId"] = k;
                row["TimeStamp"] = timeStamp;

                for (int j = 0; j < 60; j++)
                {
                    Series series = pqdifFile.Channels[0].ValueSeries[0];
                    double tempy = ((double)series.OriginalValues[i * 60 + j]+ Random.Shared.NextDouble()/100)/50*10000;
                    row[$"Freq{j + 1}"] =  Math.Round(tempy);
                }

                table.Rows.Add(row);
            }
        }

        await bulkCopy.WriteToServerAsync(table);
    }

    public async Task BulkInsertTrends(PqdifFile pqdifFile)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using SqlBulkCopy bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = "Trends"
        };

        DataTable table = new DataTable();
        table.Columns.Add("RecordingId", typeof(int));
        table.Columns.Add("TimeStamp", typeof(DateTime));

    }



    public async Task SQLInsertEvents(PqdifFile pqdifFile)
    {
       /*  CREATE TABLE PqEvents (
    PqId INT IDENTITY(1,1) PRIMARY KEY,
    TypeId INT NOT NULL,
    RecordingId INT NOT NULL,
    StartTime DATETIME2(7) NOT NULL,
    EndTime DATETIME2(7) NOT NULL,
    
    -- Compressed waveform data (stored as compressed binary)
    Timestamp VARBINARY(MAX) NOT NULL,
    U1 VARBINARY(MAX) NULL,
    U2 VARBINARY(MAX) NULL,
    U3 VARBINARY(MAX) NULL,
    UN VARBINARY(MAX) NULL,
    U12 VARBINARY(MAX) NULL,
    U23 VARBINARY(MAX) NULL,
    U31 VARBINARY(MAX) NULL,
    I1 VARBINARY(MAX) NULL,
    I2 VARBINARY(MAX) NULL,
    I3 VARBINARY(MAX) NULL,
    [IN] VARBINARY(MAX) NULL
) WITH (DATA_COMPRESSION = PAGE); */
        using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using SqlBulkCopy bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = "PqEvents"
        };
        Channel[] channels = pqdifFile.Channels;
        DataTable table = new DataTable();
        table.Columns.Add("TypeId", typeof(int));
        table.Columns.Add("RecordingId", typeof(int));
        table.Columns.Add("StartTime", typeof(DateTime));
        table.Columns.Add("EndTime", typeof(DateTime));
        table.Columns.Add("Timestamp", typeof(byte[]));
        object[] timestampValues = channels[0].TimeSeries.OriginalValues.ToArray();
        Byte[]  timestamp = CompresssionHandler.CompressWaveform(timestampValues);

        for(int i = 0; i < channels.Length; i+=2)
        {
            Channel channel = channels[i];
            if (channel.ChannelName.Contains("Power") || channel.ChannelName.Contains("Frequency") || channel.Phase==Phase.NG || channel.Phase==Phase.Net || channel.Phase.ToString().Contains("General")) continue;

            Series series = channel.ValueSeries[0];
            QuantityMeasured quantityMeasured = channel.QuantityMeasured;
            string convertedPhase = PhaseConverter.ConvertPhase(channel.Phase.ToString(), quantityMeasured.ToString());
            table.Columns.Add(convertedPhase, typeof(byte[]));


        }

        foreach (DataColumn col in table.Columns) bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);

        for(int k = 0; k <= 120; k++)
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
                    
                    if (channel.ChannelName.Contains("Power") || channel.ChannelName.Contains("Frequency") || channel.Phase==Phase.NG || channel.Phase==Phase.Net || channel.Phase.ToString().Contains("General")) continue;

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


        await bulkCopy.WriteToServerAsync(table);









    }
}
