using System.Data;
using System.Reflection.Metadata;

/// <summary>
/// Builds a DataTable schema for storing harmonic or interharmonic measurement data.
/// Supports dynamic column generation for phases and neutral, with customizable prefix and suffix.
/// </summary>
public class HarmonicsTableBuilder : IDataTableBuilder
{
    private readonly DataTable _table = new();
    private readonly int _maxHarmonics;
    private readonly string _columnPrefix;
    private readonly bool _includeNeutral;
    private readonly string _suffix;

    /// <summary>
    /// Initializes a new instance for building harmonics/interharmonics tables.
    /// </summary>
    /// <param name="columnPrefix">Prefix for column names (e.g., 'U' or 'I').</param>
    /// <param name="maxHarmonics">Maximum number of harmonics to include.</param>
    /// <param name="isInterharmonic">If true, builds for interharmonics (suffix 'IH').</param>
    public HarmonicsTableBuilder(string columnPrefix, int maxHarmonics, bool isInterharmonic = false)
    {
        _columnPrefix = columnPrefix;
        _maxHarmonics = maxHarmonics;
        _includeNeutral = true;
        _suffix = isInterharmonic ? "IH" : "H";
    }

    /// <summary>
    /// Builds and returns the DataTable schema for harmonics/interharmonics.
    /// </summary>
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

    /// <summary>
    /// Adds phase and phase-to-phase columns for a given harmonic index.
    /// </summary>
    /// <param name="harmonicIndex">The harmonic or interharmonic index.</param>
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
