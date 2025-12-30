using System.Data;

/// <summary>
/// Defines a contract for bulk inserting measurement data into a database table.
/// Implementations should handle efficient data transfer from DataTable to the target storage.
/// </summary>
public interface IMeasurementRepository
{
    /// <summary>
    /// Performs a bulk insert of all rows in the provided DataTable into the specified table.
    /// </summary>
    /// <param name="tableName">The name of the table to insert into.</param>
    /// <param name="dataTable">The DataTable containing data to insert.</param>
    Task BulkInsertAsync(string tableName, DataTable dataTable);
}