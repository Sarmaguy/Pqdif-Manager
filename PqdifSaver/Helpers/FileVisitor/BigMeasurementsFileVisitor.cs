using PQDIF_Manager;

public class BigMeasurementsFileVisitor : AbstractFileVisitor
{
    public BigMeasurementsFileVisitor()
    {
        AddRule(
            filePath => Path.GetExtension(filePath).Equals(".pqd", StringComparison.OrdinalIgnoreCase) && filePath.Contains("10Min_ClassA_PQDIF") && !filePath.Contains("2025-10-26"),//treba rjesiti DST bug, 
            async filePath =>
            {
                Console.WriteLine($"Processing big measurements file: {filePath}");
                PqdifFile pqdifFile = await PqdifFile.LoadFromFileAsync(filePath);
                SqlServerMeasurementRepository measurementRepository =
                    new SqlServerMeasurementRepository(
                        "Server=localhost\\SQLEXPRESS;Database=Pqdif;Trusted_Connection=True;TrustServerCertificate=True;");
                await measurementRepository.BulkInsertBigAsync(pqdifFile);
                Console.WriteLine($"Finished uploading big measurements from: {filePath}");
            });
    }
}