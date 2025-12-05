using PQDIF_Manager;

public class BaseFileVisitor : AbstractFileVisitor
{
    public BaseFileVisitor()
    {
        AddRule(
            filePath => Path.GetExtension(filePath).Equals(".pqd", StringComparison.OrdinalIgnoreCase) && filePath.Contains("10Min_ClassA_PQDIF") && !filePath.Contains("2025-10-26"),//treba rjesiti DST bug, 
            async filePath =>
            {
                Console.WriteLine($"Processing base measurements file: {filePath}");
                PqdifFile pqdifFile = await PqdifFile.LoadFromFileAsync(filePath);
                IMeasurementRepository measurementRepository = new SqlServerMeasurementRepository();
                Inserts inserts = new Inserts(measurementRepository);
                Console.WriteLine($"Finished uploading base measurements from: {filePath}");
            });
    }
}