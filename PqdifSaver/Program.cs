// See https://aka.ms/new-console-template for more information
using Gemstone.PQDIF.Logical;
using Microsoft.VisualBasic;
using PQDIF_Manager;




AbstractFileVisitor fileVisitor = new AbstractFileVisitor();
fileVisitor
    .AddRule(
            filePath => Path.GetExtension(filePath).Equals(".pqd", StringComparison.OrdinalIgnoreCase) && filePath.Contains("10Min_ClassA_PQDIF") && !filePath.Contains("2025-10-26"),//treba rjesiti DST bug, 
            async filePath =>
            {
                Console.WriteLine($"Processing base measurements file: {filePath}");
                PqdifFile pqdifFile = await PqdifFile.LoadFromFileAsync(filePath);
                SqlServerMeasurementRepository measurementRepository = new SqlServerMeasurementRepository();
                await measurementRepository.BulkInsertBaseAsync(pqdifFile);
                Console.WriteLine($"Finished uploading base measurements from: {filePath}");
            });
string rootFolder = @"C:\Users\Jura\Desktop\P3003845"; //lokacija foldera

await fileVisitor.VisitDirectoryAsync(rootFolder);



