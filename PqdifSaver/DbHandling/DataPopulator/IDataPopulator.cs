using System.Data;
using PQDIF_Manager;

/// <summary>
/// Defines a contract for populating a DataTable with data from a PQDIF file.
/// Implementations should extract and transform PQDIF data into the provided table schema.
/// </summary>
public interface IDataPopulator
{
    /// <summary>
    /// Populates the specified DataTable with data extracted from the given PQDIF file.
    /// </summary>
    /// <param name="table">The DataTable to populate.</param>
    /// <param name="pqdifFile">The PQDIF file containing the source data.</param>
    Task PopulateAsync(DataTable table, PqdifFile pqdifFile);
}