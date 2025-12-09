// See https://aka.ms/new-console-template for more information
using DuckDB.NET.Data;
using FluentFTP;
using Gemstone.PQDIF.Logical;
using Microsoft.VisualBasic;
using PQDIF_Manager;




/* AbstractFileVisitor fileVisitor = new AbstractFileVisitor();
fileVisitor
    .AddRule(
            filePath => Path.GetExtension(filePath).Equals(".pqd", StringComparison.OrdinalIgnoreCase) && filePath.Contains("Voltage") && !filePath.Contains("MAGDUR_PQDIF") && filePath.Contains(@"06-06"),//treba rjesiti DST bug, 
            async filePath =>
            {
                Console.WriteLine($"Processing harmonics measurements file: {filePath}");
                PqdifFile pqdifFile = await PqdifFile.LoadFromFileAsync(filePath);
                PqdifInspector.DumpRecord(pqdifFile.ObservationRecord.PhysicalRecord);//prvi record je uvijek file header
                SqlServerMeasurementRepository measurementRepository = new SqlServerMeasurementRepository();
                SqlServerMeasurementRepository.n+=120;
                SqlServerMeasurementRepository.size += ((int)new FileInfo(filePath).Length)*120;
                Console.WriteLine($"Current processed files: {SqlServerMeasurementRepository.n}, total size: {SqlServerMeasurementRepository.size / ( 1024*1024)} MB");
                await measurementRepository.SQLInsertEvents(pqdifFile);
                Console.WriteLine($"Finished uploading harmonics measurements from: {filePath}");
            });


string rootFolder = @"C:\Users\Jura\Desktop\P3003845"; //lokacija foldera
*/

ProxyFTPClient client = new ProxyFTPClient();
List<string> l = await client.DownloadPqdFilesWithEdgeCaseAsync( DateTime.Parse("14.11.2025. 23:59"),@"C:\Users\Jura\Desktop\Target" );

foreach (var file in l)
    {
        Console.WriteLine(file);
    }
