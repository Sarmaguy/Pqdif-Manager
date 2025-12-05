using System.Data;
using DuckDB.NET.Data;
using System.Threading.Tasks;

public class DuckDbMeasurementRepository : IMeasurementRepository
{
    private readonly string _connectionString;

    public DuckDbMeasurementRepository()
    {
        _connectionString = ConfigBuilder.Instance.DuckDBConnectionString;        
    }

    public async Task BulkInsertAsync(string tableName, DataTable dataTable)
    {
        using var connection = new DuckDBConnection(_connectionString);
        await connection.OpenAsync();

        // Get table schema to know all column names in correct order
        var tableColumns = new List<string>();
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = $"PRAGMA table_info('{tableName}')";
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tableColumns.Add(reader.GetString(1)); // Column name is at index 1
            }
        }

        // Create a mapping dictionary for quick lookup
        var columnMapping = new Dictionary<string, int>();
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            columnMapping[dataTable.Columns[i].ColumnName] = i;
        }

        // Create appender for efficient bulk insert
        using var appender = connection.CreateAppender(tableName);

        foreach (DataRow row in dataTable.Rows)
        {
            var appendRow = appender.CreateRow();
            
            // Append values for ALL columns in the table, in the correct order
            for (int i = 0; i < tableColumns.Count; i++)
            {
                var columnName = tableColumns[i];
                
                // Check if this column exists in our DataTable
                if (!columnMapping.ContainsKey(columnName))
                {
                    // Column doesn't exist in DataTable, append null
                    appendRow.AppendNullValue();
                    continue;
                }
                
                var dataTableColumnIndex = columnMapping[columnName];
                var value = row[dataTableColumnIndex];
                
                if (value == DBNull.Value || value == null)
                {
                    appendRow.AppendNullValue();
                }
                else
                {
                    // Cast to specific type based on column type
                    var columnType = dataTable.Columns[dataTableColumnIndex].DataType;
                    
                    if (columnType == typeof(int))
                        appendRow.AppendValue((int)value);
                    else if (columnType == typeof(long))
                        appendRow.AppendValue((long)value);
                    else if (columnType == typeof(short))
                        appendRow.AppendValue((short)value);
                    else if (columnType == typeof(byte))
                        appendRow.AppendValue((byte)value);
                    else if (columnType == typeof(double))
                        appendRow.AppendValue((double)value);
                    else if (columnType == typeof(float))
                        appendRow.AppendValue((float)value);
                    else if (columnType == typeof(decimal))
                        appendRow.AppendValue((decimal)value);
                    else if (columnType == typeof(string))
                        appendRow.AppendValue((string)value);
                    else if (columnType == typeof(bool))
                        appendRow.AppendValue((bool)value);
                    else if (columnType == typeof(DateTime))
                        appendRow.AppendValue((DateTime)value);
                    else if (columnType == typeof(Guid))
                        appendRow.AppendValue((Guid)value);
                    else if (columnType == typeof(byte[]))
                        appendRow.AppendValue((byte[])value);
                    else
                        appendRow.AppendValue(value.ToString());
                }
            }
            
            appendRow.EndRow();
        }
    }
}
