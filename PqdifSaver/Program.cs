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
string path = @"C:\Users\Jura\Desktop\P3003845\2025\Month_06\Day_01\2025-06-01_Trends-Stats\PQDIF\P3003845_2025-06-01_Trends-Stats_PQDIF.pqd";

Inserts inserts = new Inserts(new DuckDbMeasurementRepository());
PqdifFile pqdifFile = await PqdifFile.LoadFromFileAsync(path);
await inserts.BulkInsertBaseAsync(pqdifFile);

using (var connection = new DuckDBConnection(ConfigBuilder.Instance.DuckDBConnectionString))
        {
            connection.Open();

            // List all tables
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SHOW TABLES;";
                using (var reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("Tables in database:");
                    while (reader.Read())
                    {
                        Console.WriteLine(reader.GetString(0));
                    }
                }
            }

            using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM base LIMIT 10;";
                    using (var reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("\nFirst 10 rows from VoltageHarmonics:");

                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string columnName = reader.GetName(i);  // Get column name
                                object value = reader.GetValue(i);      // Get column value
                                Console.Write($"{columnName}: {value}  ");
                            }
                            Console.WriteLine(); // New line after each row
                        }
                    }
                }
        }

/* using (var connection = new DuckDBConnection(ConfigBuilder.Instance.DuckDBConnectionString))
{
    connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "TRUNCATE TABLE VoltageHarmonics;";
        cmd.CommandText = "TRUNCATE TABLE CurrentHarmonics;";
        cmd.CommandText = "TRUNCATE TABLE VoltageInterharmonics;";
        cmd.CommandText = "TRUNCATE TABLE CurrentInterharmonics;";
        cmd.ExecuteNonQuery();
} */

