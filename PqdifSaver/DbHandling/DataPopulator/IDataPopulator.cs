using System.Data;
using PQDIF_Manager;

public interface IDataPopulator
{
    Task PopulateAsync(DataTable table, PqdifFile pqdifFile);
}