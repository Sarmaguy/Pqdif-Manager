using PQDIF_Manager;

/// <summary>
/// File visitor for processing base measurement PQDIF files using rule-based actions.
/// Inherits from AbstractFileVisitor and adds a rule for 10Min_ClassA_PQDIF files.
/// </summary>
public class BaseFileVisitor : AbstractFileVisitor
{
    /// <summary>
    /// Initializes the BaseFileVisitor and adds a rule for processing base measurement files.
    /// </summary>
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