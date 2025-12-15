using System.Data;

public class EventTableBuilder : IDataTableBuilder
{
    public DataTable Build()
    {
        var table = new DataTable("PqEvents");

        table.Columns.Add("TypeId", typeof(int));
        table.Columns.Add("RecordingId", typeof(int));
        table.Columns.Add("StartTime", typeof(DateTime));
        table.Columns.Add("EndTime", typeof(DateTime));

        // Binary blob waveform columns
        table.Columns.Add("Timestamp", typeof(byte[]));
        table.Columns.Add("U1", typeof(byte[]));
        table.Columns.Add("U2", typeof(byte[]));
        table.Columns.Add("U3", typeof(byte[]));
        table.Columns.Add("UN", typeof(byte[]));
        table.Columns.Add("U12", typeof(byte[]));
        table.Columns.Add("U23", typeof(byte[]));
        table.Columns.Add("U31", typeof(byte[]));
        table.Columns.Add("I1", typeof(byte[]));
        table.Columns.Add("I2", typeof(byte[]));
        table.Columns.Add("I3", typeof(byte[]));
        table.Columns.Add("IN", typeof(byte[]));  

        return table;
    }
}