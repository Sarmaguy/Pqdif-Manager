using System.Data;
using PQDIF_Manager;

/// <summary>
/// Provides high-level bulk insert operations for harmonics, events, base, and frequency measurement data.
/// Handles table creation, data population, and repository interaction.
/// THIS IS TEMPORARY FOR TESTING PURPOSES ONLY!!! CODE WILL BE REFACTORED LATER IN INDIVIDUAL SERVICES
/// </summary>
public class Inserts
{
    private readonly IMeasurementRepository _MeasurementRepository;
    private readonly ValueRandomizer _randomizer = new();

    /// <summary>
    /// Initializes a new instance of the Inserts class with the specified measurement repository.
    /// </summary>
    /// <param name="measurementRepository">The repository to use for bulk inserts.</param>
    public Inserts(IMeasurementRepository measurementRepository)
    {
        _MeasurementRepository = measurementRepository;
    }

    /// <summary>
    /// Populates and bulk-inserts harmonics and interharmonics data for voltage and current.
    /// </summary>
    /// <param name="pqdifFile">The PQDIF file containing measurement data.</param>
    public async Task BulkInsertHarmonicsAsync(PqdifFile pqdifFile)
    {
        var tables = CreateHarmonicsTables();
        var populator = new HarmonicsDataPopulator(pqdifFile);

        await populator.PopulateAsync(tables.VoltageHarmonics, pqdifFile);
        await populator.PopulateAsync(tables.VoltageInterharmonics, pqdifFile);
        await populator.PopulateAsync(tables.CurrentHarmonics, pqdifFile);
        await populator.PopulateAsync(tables.CurrentInterharmonics, pqdifFile);

        await Task.WhenAll(
            _MeasurementRepository.BulkInsertAsync("VoltageHarmonicsNew", tables.VoltageHarmonics),
            _MeasurementRepository.BulkInsertAsync("VoltageInterharmonicsNew", tables.VoltageInterharmonics),
            _MeasurementRepository.BulkInsertAsync("CurrentHarmonicsNew", tables.CurrentHarmonics),
            _MeasurementRepository.BulkInsertAsync("CurrentInterharmonicsNew", tables.CurrentInterharmonics)
        );
    }

    /// <summary>
    /// Populates and bulk-inserts event data from the PQDIF file.
    /// </summary>
    /// <param name="pqdifFile">The PQDIF file containing event data.</param>
    public async Task BulkInsertEventsAsync(PqdifFile pqdifFile)
    {
        var dataBuilder = new EventTableBuilder();
        var populator = new EventTablePopulator();
        var table = dataBuilder.Build();

        await populator.PopulateAsync(table, pqdifFile);

        await _MeasurementRepository.BulkInsertAsync(table.TableName, table);
    }

    /// <summary>
    /// Populates and bulk-inserts base measurement data from the PQDIF file.
    /// </summary>
    /// <param name="pqdifFile">The PQDIF file containing base measurement data.</param>
    public async Task BulkInsertBaseAsync(PqdifFile pqdifFile)
    {
        var table = CreateBaseDataTable(pqdifFile.Channels);
        var populator = new BaseDataPopulator();
        
        await populator.PopulateAsync(table, pqdifFile);
        await _MeasurementRepository.BulkInsertAsync("base", table);
    }

    /// <summary>
    /// Creates DataTables for voltage/current harmonics and interharmonics.
    /// </summary>
    /// <returns>Tuple of DataTables for voltage harmonics, voltage interharmonics, current harmonics, and current interharmonics.</returns>
    private (DataTable VoltageHarmonics, DataTable VoltageInterharmonics, 
             DataTable CurrentHarmonics, DataTable CurrentInterharmonics) CreateHarmonicsTables()
    {
        return (
            new HarmonicsTableBuilder("U", MeasurementConstants.MaxHarmonics).Build(),
            new HarmonicsTableBuilder("U", MeasurementConstants.MaxInterharmonics + 1, true).Build(),
            new HarmonicsTableBuilder("I", MeasurementConstants.MaxHarmonics).Build(),
            new HarmonicsTableBuilder("I", MeasurementConstants.MaxInterharmonics + 1, true).Build()
        );
    }

    /// <summary>
    /// Creates a DataTable for base measurement data, with columns for each channel/series.
    /// </summary>
    /// <param name="channels">Array of channels to analyze for columns.</param>
    /// <returns>A DataTable with appropriate columns for base data.</returns>
    private DataTable CreateBaseDataTable(Channel[] channels)
    {
        var table = new DataTable();
        table.Columns.Add("RecordingId", typeof(short));
        table.Columns.Add("Time", typeof(DateTime));

        foreach (var channel in channels)
        {
            if (channel.Phase.ToString().Contains("LineTo"))
                continue;

            foreach (var series in channel.ValueSeries)
            {
                if (series.QuantityCharacteristic != null && 
                    series.QuantityCharacteristic.StartsWith("Spectra by"))
                    continue;

                if ((series.QuantityCharacteristic != null) && (series.QuantityCharacteristic.Contains("Negative sequence component unbalance (%)") || 
                    series.QuantityCharacteristic.Contains("Zero sequence component unbalance (%)")))
                    series.QuantityCharacteristic = $"{channel.QuantityMeasured} {series.QuantityCharacteristic}";

                var columnName = MeasurementTypes.GetTableColumn(
                    channel.Phase,
                    series.SeriesValueType,
                    series.QuantityUnits.ToString(),
                    series.QuantityCharacteristic
                );

                if (columnName != null && !columnName.Contains("Hx"))
                {
                    table.Columns.Add(columnName, typeof(int));
                }
            }
        }

        return table;
    }


    public async Task BulkInsertFreq60(PqdifFile pqdifFile)
    {
        var table = CreateFrequencyTable(MeasurementConstants.FrequencySampleSize60, typeof(int));
        PopulateFrequencyTable(table, pqdifFile, MeasurementConstants.FrequencySampleSize60, true, 1);
        await _MeasurementRepository.BulkInsertAsync("Frequency60ColumnstoreInt", table);
    }

    /// <summary>
    /// Populates and bulk-inserts 60 Hz frequency measurement data.
    /// </summary>
    /// <param name="pqdifFile">The PQDIF file containing frequency data.</param>
    /// <summary>
    /// Populates and bulk-inserts 720 Hz frequency measurement data.
    /// </summary>
    /// <param name="pqdifFile">The PQDIF file containing frequency data.</param>
    public async Task BulkInsertFreq720(PqdifFile pqdifFile)
    {
        var table = CreateFrequencyTable(MeasurementConstants.FrequencySampleSize720, typeof(int));
        PopulateFrequencyTable(table, pqdifFile, MeasurementConstants.FrequencySampleSize720, true, 0);
        await _MeasurementRepository.BulkInsertAsync("Frequency720Int", table);
    }

    /// <summary>
    /// Creates a DataTable for frequency measurement data with the specified sample size and data type.
    /// </summary>
    /// <param name="sampleSize">Number of frequency samples per row.</param>
    /// <param name="dataType">Type of the frequency columns (e.g., int or double).</param>
    /// <returns>A DataTable with columns for frequency data.</returns>
    private DataTable CreateFrequencyTable(int sampleSize, Type dataType)
    {
        var table = new DataTable();
        table.Columns.Add("RecordingId", typeof(int));
        table.Columns.Add("TimeStamp", typeof(DateTime));
        table.Columns.Add("F_AVG",typeof(int));
        table.Columns.Add("F_MIN", dataType);
        table.Columns.Add("F_MAX", dataType);

        for (int i = 1; i <= sampleSize; i++)
        {
            table.Columns.Add($"Freq{i}", dataType);
        }

        return table;
    }



    /// <summary>
    /// Populates a frequency DataTable with values from the PQDIF file, calculating min, max, and average.
    /// </summary>
    /// <param name="table">The DataTable to populate.</param>
    /// <param name="pqdifFile">The PQDIF file containing frequency data.</param>
    /// <param name="sampleSize">Number of frequency samples per row.</param>
    /// <param name="asInteger">Whether to store values as integers.</param>
    /// <param name="yearOffset">Offset to apply to the start time (for test data separation).</param>
    private void PopulateFrequencyTable(DataTable table, PqdifFile pqdifFile, 
        int sampleSize, bool asInteger, int yearOffset)
    {
        var series = pqdifFile.Channels[0].ValueSeries[0];
        int totalMeasurements = pqdifFile.Channels[0].TimeSeries.SampleCount / sampleSize;
        var startTime = pqdifFile.StartTime.AddYears(yearOffset);

        for (int i = 0; i < totalMeasurements; i++)
        {
            var timestamp = startTime
                .AddSeconds((double)pqdifFile.Channels[0].TimeSeries.OriginalValues[i * sampleSize])
                .ToUniversalTime();

            for (int recordingId = 0; recordingId < 1; recordingId++)
            {
                var row = table.NewRow();
                row["RecordingId"] = recordingId;
                row["TimeStamp"] = timestamp;
                double min = double.MaxValue;
                double max = double.MinValue;
                double sum = 0.0;

                for (int j = 0; j < sampleSize; j++)
                {
                    double rawValue = (double)series.OriginalValues[i * sampleSize + j];
                    
                    if (asInteger)
                    {
                        var intVal = (int)Math.Round(_randomizer.AdjustValueAsInt(rawValue)* 1000d);
                        row[$"Freq{j + 1}"] = intVal;
                        sum += intVal;
                        if (intVal < min) min = intVal;
                        if (intVal > max) max = intVal;
                    }
                    else
                    {
                        var dblVal = _randomizer.AdjustValue(rawValue, 0.01);
                        row[$"Freq{j + 1}"] = dblVal;
                        sum += dblVal;
                        if (dblVal < min) min = dblVal;
                        if (dblVal > max) max = dblVal;
                    }
                }

                var avg = sum / sampleSize;
                if (asInteger)
                {
                    row["F_AVG"] = (int)Math.Round(avg);
                    row["F_MIN"] = (int)Math.Round(min);
                    row["F_MAX"] = (int)Math.Round(max);
                }
                else
                {

                    row["F_AVG"] = (int)Math.Round(avg);
                    row["F_MIN"] = min;
                    row["F_MAX"] = max;
                }

                table.Rows.Add(row);
            }
        }
    }
}