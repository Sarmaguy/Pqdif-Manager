using System.Data;
using System.Reflection.Metadata;

public class HarmonicsTableBuilder : IDataTableBuilder
{
    private readonly DataTable _table = new();
    private readonly int _maxHarmonics;
    private readonly string _columnPrefix;
    private readonly bool _includeNeutral;
    private readonly string _suffix;

    public HarmonicsTableBuilder(string columnPrefix, int maxHarmonics, bool isInterharmonic = false)
    {
        _columnPrefix = columnPrefix;
        _maxHarmonics = maxHarmonics;
        _includeNeutral = true;
        _suffix = isInterharmonic ? "IH" : "H";
    }

    public DataTable Build()
    {
        _table.Columns.Add("RecordingId", typeof(short));
        _table.Columns.Add("TimeStamp", typeof(DateTime));

        for (int i = 0; i < _maxHarmonics; i++)
        {
            if( i == 0 && _suffix == "IH")
                continue;
            AddPhaseColumns(i);
            if (_includeNeutral)
                _table.Columns.Add($"{_columnPrefix}N{_suffix}{i}", typeof(int));

        }

        return _table;
    }

    private void AddPhaseColumns(int harmonicIndex)
    {
        for (int phase = 1; phase <= 3; phase++)
        {
            // Phase-to-neutral: U1H0, U2H0, U3H0
            _table.Columns.Add($"{_columnPrefix}{phase}{_suffix}{harmonicIndex}", typeof(int));

            if(_columnPrefix != "U") continue;
            
            // Phase-to-phase: U12H0, U23H0, U31H0
            int nextPhase = (phase % 3) + 1;
            _table.Columns.Add($"{_columnPrefix}{phase}{nextPhase}{_suffix}{harmonicIndex}", typeof(int));
        }
}
}
