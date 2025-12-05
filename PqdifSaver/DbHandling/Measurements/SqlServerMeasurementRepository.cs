using System.Data;
using Gemstone.PQDIF.Logical;
using Microsoft.Data.SqlClient;
using PQDIF_Manager;
using System.Text.RegularExpressions;
using System.Numerics;
public class SqlServerMeasurementRepository : IMeasurementRepository
{
    private readonly string _connectionString;
    public static int size = 0;
    public static BigInteger n = 0;

    public SqlServerMeasurementRepository()
    {
        _connectionString = ConfigBuilder.Instance.ConnectionString;
    }

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
