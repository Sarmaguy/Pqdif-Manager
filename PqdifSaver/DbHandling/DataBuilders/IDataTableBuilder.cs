using System.Data;

/// <summary>
/// Defines the contract for building and managing a specific data table in the database.
/// Implementations provide SQL for table creation that will return a valid DataTable schema, after calling Build().
/// </summary>
public interface IDataTableBuilder
{
    DataTable Build();
}