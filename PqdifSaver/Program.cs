// See https://aka.ms/new-console-template for more information
using DuckDB.NET.Data;
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




    await fileVisitor.VisitDirectoryAsync(rootFolder); */
DuckDbManager.CreateTables();
string path = @"C:\Users\Jura\Desktop\P3003845\2025\Month_06\Day_04\T_07-58-28-394_Voltage_Sag\PQDIF\P3003845_2025-06-04_T_07-58-28-394_Voltage_Sag_PQDIF.pqd";

Inserts inserts = new Inserts(new DuckDbMeasurementRepository());
PqdifFile pqdifFile = await PqdifFile.LoadFromFileAsync(path);
await inserts.BulkInsertEventsAsync(pqdifFile);


/* using (var connection = new DuckDBConnection(ConfigBuilder.Instance.DuckDBConnectionString))
{
    connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "Drop TABLE PqEvents";
        cmd.ExecuteNonQuery();
} */

