using System.Data;
using Gemstone.PQDIF.Logical;
using Microsoft.Data.SqlClient;
using PQDIF_Manager;
using System.Text.RegularExpressions;
using System.Numerics;
/// <summary>
/// Implements IMeasurementRepository for bulk inserting measurement data into a SQL Server database.
/// Uses SqlBulkCopy for efficient data transfer from DataTable to SQL table.
/// </summary>
public class SqlServerMeasurementRepository : IMeasurementRepository
{
    private readonly string _connectionString;
    public static int size = 0;
    public static BigInteger n = 0;

    /// <summary>
    /// Initializes a new repository using the SQL Server connection string from configuration.
    /// </summary>
    public SqlServerMeasurementRepository()
    {
        _connectionString = ConfigBuilder.Instance.ConnectionString;
    }

    /// <summary>
    /// Performs a bulk insert of all rows in the provided DataTable into the specified SQL Server table.
    /// Uses SqlBulkCopy for high-performance data loading.
    /// </summary>
    /// <param name="tableName">The name of the SQL Server table to insert into.</param>
    /// <param name="dataTable">The DataTable containing data to insert.</param>
    public async Task BulkInsertAsync(string tableName, DataTable dataTable)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = tableName
        };

        foreach (DataColumn col in dataTable.Columns)
        {
            bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
        }

        await bulkCopy.WriteToServerAsync(dataTable);
    }
}
