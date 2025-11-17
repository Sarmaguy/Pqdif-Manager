public interface IMeasurementRepository
{
    Task BulkInsertAsync(IEnumerable<Measurement> measurements);
}