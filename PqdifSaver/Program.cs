// See https://aka.ms/new-console-template for more information
using Gemstone.PQDIF.Logical;
using Microsoft.VisualBasic;
using PQDIF_Manager;




PqdifFile pqdifFile = await PqdifFile.LoadFromFileAsync(@"C:\Users\Jura\Desktop\PSL_P3003845_2025-04-12_10Min_ClassA_PQDIF.pqd");
IMeasurementRepository measurementRepository =
    new SqlServerMeasurementRepository(
        "Server=localhost\\SQLEXPRESS;Database=Pqdif;Trusted_Connection=True;TrustServerCertificate=True;");

Console.WriteLine("Starting to upload measurements...");
await measurementRepository.BulkInsertAsync(await pqdifFile.ParseMeasurementsFromFile());
Console.WriteLine("Finished uploading measurements.");

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

