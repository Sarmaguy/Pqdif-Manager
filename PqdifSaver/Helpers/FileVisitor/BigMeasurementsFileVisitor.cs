using PQDIF_Manager;

/// <summary>
/// File visitor for processing large measurement PQDIF files using rule-based actions.
/// Inherits from AbstractFileVisitor and adds a rule for 10Min_ClassA_PQDIF files.
/// </summary>
public class BigMeasurementsFileVisitor : AbstractFileVisitor
{
    /// <summary>
    /// Initializes the BigMeasurementsFileVisitor and adds a rule for processing big measurement files.
    /// </summary>
    public BigMeasurementsFileVisitor()
    {
        AddRule(
            filePath => Path.GetExtension(filePath).Equals(".pqd", StringComparison.OrdinalIgnoreCase) && filePath.Contains("10Min_ClassA_PQDIF") && !filePath.Contains("2025-10-26"),//treba rjesiti DST bug, 
            async filePath =>
            {
                Console.WriteLine($"Processing big measurements file: {filePath}");
                PqdifFile pqdifFile = await PqdifFile.LoadFromFileAsync(filePath);
                SqlServerMeasurementRepository measurementRepository = new SqlServerMeasurementRepository();
/*                 await measurementRepository.BulkInsertBigAsync(pqdifFile); */
                Console.WriteLine($"Finished uploading big measurements from: {filePath}");
            });
    }
}