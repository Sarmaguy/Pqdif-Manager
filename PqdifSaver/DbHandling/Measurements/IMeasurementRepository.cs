using System.Data;

public interface IMeasurementRepository
{
    Task BulkInsertAsync(string tableName, DataTable dataTable);
}