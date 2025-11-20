using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Gemstone.PQDIF.Logical;

public class MeasurementTypes
{
    private static readonly Lazy<MeasurementTypes> Instance =
        new Lazy<MeasurementTypes>(() => new MeasurementTypes());

    private Dictionary<string, string> _measurementTypeMappings;

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

    public static string? GetTableColumn(string phase, string valueType, string unit, string quantityCharacteristic)
    {
        string key = $"{phase}_{valueType}_{unit}_{quantityCharacteristic}";
        if (Instance.Value._measurementTypeMappings.TryGetValue(key, out string? measurementType))
            return measurementType;
        return null;
    }

    public static string? GetTableColumn(Phase phase, string valueType, string unit, string quantityCharacteristic)
    {
        return GetTableColumn(phase.ToString(), valueType, unit, quantityCharacteristic);
    }

    public static string[] GetAllTableColumns()
    {
        return Instance.Value._measurementTypeMappings.Values.ToArray();
    }
}