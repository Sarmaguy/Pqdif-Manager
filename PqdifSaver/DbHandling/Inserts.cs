using System.Data;
using PQDIF_Manager;

public class Inserts
{
    private readonly IMeasurementRepository _MeasurementRepository;
    private readonly ValueRandomizer _randomizer = new();

    public Inserts(IMeasurementRepository measurementRepository)
    {
        _MeasurementRepository = measurementRepository;
    }

    public async Task BulkInsertHarmonicsAsync(PqdifFile pqdifFile)
    {
        var tables = CreateHarmonicsTables();
        var populator = new HarmonicsDataPopulator();

        await populator.PopulateAsync(tables.VoltageHarmonics, pqdifFile);
        await populator.PopulateAsync(tables.VoltageInterharmonics, pqdifFile);
        await populator.PopulateAsync(tables.CurrentHarmonics, pqdifFile);
        await populator.PopulateAsync(tables.CurrentInterharmonics, pqdifFile);

        await Task.WhenAll(
            _MeasurementRepository.BulkInsertAsync("VoltageHarmonics", tables.VoltageHarmonics),
            _MeasurementRepository.BulkInsertAsync("VoltageInterharmonics", tables.VoltageInterharmonics),
            _MeasurementRepository.BulkInsertAsync("CurrentHarmonics", tables.CurrentHarmonics),
            _MeasurementRepository.BulkInsertAsync("CurrentInterharmonics", tables.CurrentInterharmonics)
        );
    }

    public async Task BulkInsertBaseAsync(PqdifFile pqdifFile)
    {
        var table = CreateBaseDataTable(pqdifFile.Channels);
        var populator = new BaseDataPopulator();
        
        await populator.PopulateAsync(table, pqdifFile);
        await _MeasurementRepository.BulkInsertAsync("trend", table);
    }

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

    // Simplified frequency methods using the new pattern
    public async Task BulkInsertFreq60(PqdifFile pqdifFile)
    {
        var table = CreateFrequencyTable(MeasurementConstants.FrequencySampleSize60, typeof(double));
        PopulateFrequencyTable(table, pqdifFile, MeasurementConstants.FrequencySampleSize60, false, 1);
        await _MeasurementRepository.BulkInsertAsync("Frequency60Columnstore", table);
    }

    public async Task BulkInsertFreq720(PqdifFile pqdifFile)
    {
        var table = CreateFrequencyTable(MeasurementConstants.FrequencySampleSize720, typeof(int));
        PopulateFrequencyTable(table, pqdifFile, MeasurementConstants.FrequencySampleSize720, true, 0);
        await _MeasurementRepository.BulkInsertAsync("Frequency720Int", table);
    }

    private DataTable CreateFrequencyTable(int sampleSize, Type dataType)
    {
        var table = new DataTable();
        table.Columns.Add("RecordingId", typeof(int));
        table.Columns.Add("TimeStamp", typeof(DateTime));

        for (int i = 1; i <= sampleSize; i++)
        {
            table.Columns.Add($"Freq{i}", dataType);
        }

        return table;
    }

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

            for (int recordingId = 0; recordingId < MeasurementConstants.RecordingsPerMeasurement; recordingId++)
            {
                var row = table.NewRow();
                row["RecordingId"] = recordingId;
                row["TimeStamp"] = timestamp;

                for (int j = 0; j < sampleSize; j++)
                {
                    double value = (double)series.OriginalValues[i * sampleSize + j];
                    
                    if (asInteger)
                    {
                        row[$"Freq{j + 1}"] = _randomizer.AdjustValueAsInt(value);
                    }
                    else
                    {
                        row[$"Freq{j + 1}"] = _randomizer.AdjustValue(value, 0.01);
                    }
                }

                table.Rows.Add(row);
            }
        }
    }
}