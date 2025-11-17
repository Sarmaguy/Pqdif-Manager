// See https://aka.ms/new-console-template for more information
using Gemstone.PQDIF.Logical;
using Microsoft.VisualBasic;
using PQDIF_Manager;

string rootFolder = @"C:\Users\Jura\Desktop\P3003845"; //lokacija foldera
        IMeasurementRepository measurementRepository =
            new SqlServerMeasurementRepository(
                "Server=localhost\\SQLEXPRESS;Database=Pqdif;Trusted_Connection=True;TrustServerCertificate=True;");

Console.WriteLine("Starting to upload measurements from folder...");

await ProcessDirectoryAsync(rootFolder, measurementRepository);

Console.WriteLine("Finished uploading all measurements.");

static async Task ProcessDirectoryAsync(string directoryPath, IMeasurementRepository measurementRepository) //Funkcija za fileVisitor, da automatiziram upis svih fileova
    {
        
        foreach (var filePath in Directory.GetFiles(directoryPath))
        {
            if ((filePath.Contains("10Sec_Frequency_ClassA") || filePath.Contains("Power_Energy-TP") || filePath.Contains("Trends-Stats_PQDIF") || filePath.Contains("10Min_ClassA_PQDIF")) && Path.GetExtension(filePath).Equals(".pqd", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Processing file: {filePath}");

                try
                {
                    PqdifFile pqdifFile = await PqdifFile.LoadFromFileAsync(filePath);
                    var measurements = await pqdifFile.ParseMeasurementsFromFile();
                    await measurementRepository.BulkInsertAsync(measurements);

                    Console.WriteLine($"Finished uploading: {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
                }
            }
        }

        //grananje u poddirektorije
        foreach (var subDir in Directory.GetDirectories(directoryPath))
        {
            await ProcessDirectoryAsync(subDir, measurementRepository);
        }
    }



/* PqdifFile pqdifFile = await PqdifFile.LoadFromFileAsync(@"C:\Users\Jura\Desktop\PSL_P3003845_2025-04-12_10Min_ClassA_PQDIF.pqd");
IMeasurementRepository measurementRepository =
    new SqlServerMeasurementRepository(
        "Server=localhost\\SQLEXPRESS;Database=Pqdif;Trusted_Connection=True;TrustServerCertificate=True;");

Console.WriteLine("Starting to upload measurements...");
await measurementRepository.BulkInsertAsync(await pqdifFile.ParseMeasurementsFromFile());
Console.WriteLine("Finished uploading measurements."); */

/*ISeriesInfoRepository seriesInfoSaver =
    new SqlServerSeriesInfoRepository(
        "Server=localhost\\SQLEXPRESS;Database=Pqdif;Trusted_Connection=True;TrustServerCertificate=True;");

 foreach (var channel in pqdifFile.Channels)  Ovo je skripta za napuniti SeriesInfo tablicus
{
    foreach (var series in channel.ValueSeries)
    {
        SeriesInfo seriesInfo = new SeriesInfo
        {
            ChannelName = channel.ChannelName,
            QuantityMeasured = channel.QuantityMeasured.ToString(),
            Phase = channel.Phase.ToString(),
            SeriesValueType = series.SeriesValueType
        };

        await seriesInfoSaver.GetSeriesIdAsync(seriesInfo);
        if (seriesInfo.SeriesId == 0)
        {
            await seriesInfoSaver.SaveSeriesInfoAsync(seriesInfo);
            Console.WriteLine($"Saved SeriesInfo: {seriesInfo.ChannelName}, {seriesInfo.QuantityMeasured}, {seriesInfo.Phase}, {seriesInfo.SeriesValueType} with SeriesId: {seriesInfo.SeriesId}");
        }
        else
        {
            Console.WriteLine($"SeriesInfo already exists: {seriesInfo.ChannelName}, {seriesInfo.QuantityMeasured}, {seriesInfo.Phase}, {seriesInfo.SeriesValueType} with SeriesId: {seriesInfo.SeriesId}");
        }
    }
} */

