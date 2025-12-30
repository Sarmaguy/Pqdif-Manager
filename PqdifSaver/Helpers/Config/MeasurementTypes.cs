using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Gemstone.PQDIF.Logical;

/// <summary>
/// Provides mapping from measurement metadata (phase, value type, unit, characteristic) to table column names.
/// Loads mappings from an XML resource at startup.
/// </summary>
public class MeasurementTypes
{
    private static readonly Lazy<MeasurementTypes> Instance =
        new Lazy<MeasurementTypes>(() => new MeasurementTypes());

    private Dictionary<string, string> _measurementTypeMappings;

    /// <summary>
    /// Loads measurement type mappings from the MeasurementTypes.xml resource.
    /// </summary>
    private MeasurementTypes()
    {
        var path = Path.Combine(AppContext.BaseDirectory, @"Resources\MeasurementTypes.xml");
        var xml = XDocument.Load(path);
        _measurementTypeMappings = new Dictionary<string, string>();

        foreach (var groups in xml.Descendants("groups"))
            {

                foreach (var g in groups.Elements("group"))
                {
                    string def = g.Element("columnName").Value;
                    string phase = g.Element("phase").Value;
                    string valueType = g.Element("valueType").Value;
                    string unit = g.Element("unit").Value;
                    string quantityCharacteristic = g.Element("tagQuantityId").Value;

                    if (valueType == "" || valueType == null) valueType = "Values";

                    string key = $"{phase}_{valueType}_{unit}_{quantityCharacteristic}";
                    _measurementTypeMappings[key] = def;
                    //Console.WriteLine($"Mapping added: {key} -> {def}");
                }
            }
    }

    /// <summary>
    /// Gets the table column name for the specified measurement metadata.
    /// </summary>
    /// <param name="phase">The phase (string).</param>
    /// <param name="valueType">The value type (e.g., "Values").</param>
    /// <param name="unit">The unit (e.g., "V").</param>
    /// <param name="quantityCharacteristic">The quantity characteristic/tag.</param>
    /// <returns>The mapped column name, or null if not found.</returns>
    public static string? GetTableColumn(string phase, string valueType, string unit, string quantityCharacteristic)
    {
        string key = $"{phase}_{valueType}_{unit}_{quantityCharacteristic}";
        if (Instance.Value._measurementTypeMappings.TryGetValue(key, out string? measurementType))
            return measurementType;
        return null;
    }

    /// <summary>
    /// Gets the table column name for the specified measurement metadata (using Phase enum).
    /// </summary>
    /// <param name="phase">The phase (enum).</param>
    /// <param name="valueType">The value type.</param>
    /// <param name="unit">The unit.</param>
    /// <param name="quantityCharacteristic">The quantity characteristic/tag.</param>
    /// <returns>The mapped column name, or null if not found.</returns>
    public static string? GetTableColumn(Phase phase, string valueType, string unit, string quantityCharacteristic)
    {
        return GetTableColumn(phase.ToString(), valueType, unit, quantityCharacteristic);
    }

    /// <summary>
    /// Gets all mapped table column names.
    /// </summary>
    /// <returns>Array of all column names.</returns>
    public static string[] GetAllTableColumns()
    {
        return Instance.Value._measurementTypeMappings.Values.ToArray();
    }
}