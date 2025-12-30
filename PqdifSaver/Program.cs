// See https://aka.ms/new-console-template for more information
using DuckDB.NET.Data;
using FluentFTP;
using Gemstone.PQDIF.Logical;
using Microsoft.VisualBasic;
using PQDIF_Manager;




AbstractFileVisitor fileVisitor = new AbstractFileVisitor();
fileVisitor
    .AddRule(
            filePath => Path.GetExtension(filePath).Equals(".pqd", StringComparison.OrdinalIgnoreCase) && filePath.Contains("10Min_ClassA_PQDIF") && !filePath.Contains("MAGDUR_PQDIF") && !filePath.Contains(@"06-06"),//treba rjesiti DST bug, 
            async filePath =>
            {
                Console.WriteLine($"Processing harmonics measurements file: {filePath}");
                PqdifFile pqdifFile = await PqdifFile.LoadFromFileAsync(filePath);
                SqlServerMeasurementRepository measurementRepository = new SqlServerMeasurementRepository();
                Inserts inserts = new Inserts(measurementRepository);
                await inserts.BulkInsertHarmonicsAsync(pqdifFile);
                Console.WriteLine($"Finished uploading harmonics measurements from: {filePath}");
            });


string rootFolder = @"C:\Users\Jura\Desktop\P3003845"; //lokacija foldera




    await fileVisitor.VisitDirectoryAsync(rootFolder);
/* DuckDbManager.CreateTables(); */
/* string path = @"C:\Users\Jura\Desktop\P3003845\2025\Month_05\Day_02\2025-05-02_Trends-Stats\PQDIF\P3003845_2025-05-02_10Min_ClassA_PQDIF.pqd";

Inserts inserts = new Inserts(new SqlServerMeasurementRepository());
PqdifFile pqdifFile = await PqdifFile.LoadFromFileAsync(path);
await inserts.BulkInsertHarmonicsAsync(pqdifFile); */


/* using (var connection = new DuckDBConnection(ConfigBuilder.Instance.DuckDBConnectionString))
{
    connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "Drop TABLE PqEvents";
        cmd.ExecuteNonQuery();
} */

