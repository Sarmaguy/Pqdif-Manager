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
        var populator = new HarmonicsDataPopulator(pqdifFile);

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

    public async Task BulkInsertEventsAsync(PqdifFile pqdifFile)
    {
        var dataBuilder = new EventTableBuilder();
        var populator = new EventTablePopulator();
        var table = dataBuilder.Build();

        await populator.PopulateAsync(table, pqdifFile);

        await _MeasurementRepository.BulkInsertAsync(table.TableName, table);
    }

    public async Task BulkInsertBaseAsync(PqdifFile pqdifFile)
    {
        var table = CreateBaseDataTable(pqdifFile.Channels);
        var populator = new BaseDataPopulator();
        
        await populator.PopulateAsync(table, pqdifFile);
        await _MeasurementRepository.BulkInsertAsync("base", table);
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


    public async Task BulkInsertFreq60(PqdifFile pqdifFile)
    {
        var table = CreateFrequencyTable(MeasurementConstants.FrequencySampleSize60, typeof(int));
        PopulateFrequencyTable(table, pqdifFile, MeasurementConstants.FrequencySampleSize60, true, 1);
        await _MeasurementRepository.BulkInsertAsync("Frequency60ColumnstoreInt", table);
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
        table.Columns.Add("F_AVG",typeof(int));
        table.Columns.Add("F_MIN", dataType);
        table.Columns.Add("F_MAX", dataType);

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