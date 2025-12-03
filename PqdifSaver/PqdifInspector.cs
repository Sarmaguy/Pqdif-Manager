using System;
using System.Linq;
using System.Text;
using Gemstone.PQDIF.Physical;
using Gemstone.PQDIF.Logical;
using System.Collections.Generic;



public static class PqdifInspector
{
    // epsilon for numeric match
    private const double EPS = 1e-6;
    private const double TARGET = 0.07;

    public static void DumpRecord(Record physicalRecord)
    {
        Console.WriteLine($"=== RECORD DUMP ===");
        Console.WriteLine($"Record Type: {physicalRecord.Header.TypeOfRecord}");
        Console.WriteLine($"Next Record Position: {physicalRecord.Header.NextRecordPosition}");
        Console.WriteLine($"Checksum: 0x{physicalRecord.Header.Checksum:X8}");
        Console.WriteLine($"Header Size: {physicalRecord.Header.HeaderSize} bytes");
        Console.WriteLine($"Body Size: {physicalRecord.Header.BodySize} bytes");
        Console.WriteLine();
        
        var root = physicalRecord.Body.Collection;
        DumpCollection(root, "/RecordBody", 0);
        Console.WriteLine("=== END RECORD DUMP ===");

        Console.WriteLine();
        Console.WriteLine("=== SEARCHING FOR REAL8 == 0.07 (±eps) ===");
        SearchForValueReal8(root, "/RecordBody");
        Console.WriteLine("=== END SEARCH ===");
    }

    private static void DumpCollection(CollectionElement collection, string path, int indent)
    {
        if (collection is null)
            return;

        string indentStr = new string(' ', indent * 2);

        foreach (Element element in collection.Elements)
        {
            string tagStr = ElementTagInfo(element.TagOfElement);
            string elementPath = $"{path}/{ElementTagShort(element.TagOfElement)}";

            switch (element)
            {
                case CollectionElement col:
                    Console.WriteLine($"{indentStr}[COLLECTION] {tagStr}");
                    Console.WriteLine($"{indentStr}  Path: {elementPath}");
                    Console.WriteLine($"{indentStr}  Element Count: {col.Elements.Count}");
                    DumpCollection(col, elementPath, indent + 1);
                    break;

                case ScalarElement scalar:
                    try
                    {
                        string valueStr = ScalarValueToString(scalar);
                        Console.WriteLine($"{indentStr}[SCALAR] {tagStr}");
                        Console.WriteLine($"{indentStr}  Path: {elementPath}");
                        Console.WriteLine($"{indentStr}  Physical Type: {scalar.TypeOfValue}");
                        Console.WriteLine($"{indentStr}  Value: {valueStr}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{indentStr}[SCALAR] {tagStr}");
                        Console.WriteLine($"{indentStr}  Path: {elementPath}");
                        Console.WriteLine($"{indentStr}  Physical Type: {scalar.TypeOfValue}");
                        Console.WriteLine($"{indentStr}  Value: <ERROR: {ex.Message}>");
                    }
                    break;

                case VectorElement vector:
                    try
                    {
                        string vectorInfo = VectorValueToString(vector);
                        Console.WriteLine($"{indentStr}[VECTOR] {tagStr}");
                        Console.WriteLine($"{indentStr}  Path: {elementPath}");
                        Console.WriteLine($"{indentStr}  Physical Type: {vector.TypeOfValue}");
                        Console.WriteLine($"{indentStr}  Size: {vector.Size}");
                        Console.WriteLine($"{indentStr}  Values: {vectorInfo}");
                        
                        // Additional vector statistics for numeric types
                        DumpVectorStatistics(vector, indentStr);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{indentStr}[VECTOR] {tagStr}");
                        Console.WriteLine($"{indentStr}  Path: {elementPath}");
                        Console.WriteLine($"{indentStr}  Physical Type: {vector.TypeOfValue}");
                        Console.WriteLine($"{indentStr}  Size: {vector.Size}");
                        Console.WriteLine($"{indentStr}  Values: <ERROR: {ex.Message}>");
                    }
                    break;

                default:
                    Console.WriteLine($"{indentStr}[UNKNOWN] {tagStr}");
                    Console.WriteLine($"{indentStr}  Path: {elementPath}");
                    Console.WriteLine($"{indentStr}  Element Type: {element.GetType().Name}");
                    break;
            }
            
            Console.WriteLine(); // Blank line between elements
        }
    }

    private static void DumpVectorStatistics(VectorElement vector, string indentStr)
    {
        try
        {
            switch (vector.TypeOfValue)
            {
                case PhysicalType.Real8:
                    if (vector.Size > 0)
                    {
                        double min = double.MaxValue;
                        double max = double.MinValue;
                        double sum = 0;
                        for (int i = 0; i < vector.Size; i++)
                        {
                            double val = vector.GetReal8(i);
                            if (val < min) min = val;
                            if (val > max) max = val;
                            sum += val;
                        }
                        Console.WriteLine($"{indentStr}  Stats: Min={min:G17}, Max={max:G17}, Avg={sum/vector.Size:G17}");
                    }
                    break;
                    
                case PhysicalType.Real4:
                    if (vector.Size > 0)
                    {
                        float min = float.MaxValue;
                        float max = float.MinValue;
                        double sum = 0;
                        for (int i = 0; i < vector.Size; i++)
                        {
                            float val = vector.GetReal4(i);
                            if (val < min) min = val;
                            if (val > max) max = val;
                            sum += val;
                        }
                        Console.WriteLine($"{indentStr}  Stats: Min={min:G9}, Max={max:G9}, Avg={sum/vector.Size:G9}");
                    }
                    break;
                    
                case PhysicalType.Integer4:
                case PhysicalType.UnsignedInteger4:
                    if (vector.Size > 0 && vector.Size < 10000) // reasonable size check
                    {
                        long min = long.MaxValue;
                        long max = long.MinValue;
                        for (int i = 0; i < vector.Size; i++)
                        {
                            long val = vector.TypeOfValue == PhysicalType.Integer4 
                                ? vector.GetInt4(i) 
                                : vector.GetUInt4(i);
                            if (val < min) min = val;
                            if (val > max) max = val;
                        }
                        Console.WriteLine($"{indentStr}  Stats: Min={min}, Max={max}");
                    }
                    break;
            }
        }
        catch
        {
            // Silently ignore statistics calculation errors
        }
    }

    private static string ScalarValueToString(ScalarElement scalar)
    {
        switch (scalar.TypeOfValue)
        {
            case PhysicalType.Real8:
                return scalar.GetReal8().ToString("G17");
            case PhysicalType.Real4:
                return scalar.GetReal4().ToString("G9");
            case PhysicalType.Integer4:
                return scalar.GetInt4().ToString();
            case PhysicalType.UnsignedInteger4:
                return scalar.GetUInt4().ToString();
            case PhysicalType.Integer2:
                return scalar.GetInt2().ToString();
            case PhysicalType.UnsignedInteger2:
                return scalar.GetUInt2().ToString();
            case PhysicalType.Integer1:
                return scalar.GetInt1().ToString();
            case PhysicalType.UnsignedInteger1:
                return scalar.GetUInt1().ToString();
            case PhysicalType.Timestamp:
                return scalar.GetTimestamp().ToString("o");
            case PhysicalType.Guid:
                return scalar.GetGuid().ToString();
            case PhysicalType.Complex8:
                var c8 = scalar.GetComplex8();
                return $"{c8.Real:G17} + {c8.Imaginary:G17}i";
            default:
                return $"<{scalar.TypeOfValue}>";
        }
    }

    private static string VectorValueToString(VectorElement vector)
    {
        // Handle text specially
        if (vector.TypeOfValue == PhysicalType.Char1)
        {
            byte[] bytes = vector.GetValues();
            return $"\"{Encoding.ASCII.GetString(bytes).Trim((char)0)}\"";
        }

        // For very small vectors, print all values
        int printSize = vector.Size <= 8 ? vector.Size : 5;
        var values = new List<string>();
        
        for (int i = 0; i < printSize; i++)
        {
            try
            {
                switch (vector.TypeOfValue)
                {
                    case PhysicalType.Real8:
                        values.Add(vector.GetReal8(i).ToString("G17"));
                        break;
                    case PhysicalType.Real4:
                        values.Add(vector.GetReal4(i).ToString("G9"));
                        break;
                    case PhysicalType.Integer4:
                        values.Add(vector.GetInt4(i).ToString());
                        break;
                    case PhysicalType.UnsignedInteger4:
                        values.Add(vector.GetUInt4(i).ToString());
                        break;
                    case PhysicalType.Integer2:
                        values.Add(vector.GetInt2(i).ToString());
                        break;
                    case PhysicalType.UnsignedInteger2:
                        values.Add(vector.GetUInt2(i).ToString());
                        break;
                    case PhysicalType.Integer1:
                        values.Add(vector.GetInt1(i).ToString());
                        break;
                    case PhysicalType.UnsignedInteger1:
                        values.Add(vector.GetUInt1(i).ToString());
                        break;
                    case PhysicalType.Guid:
                        values.Add(vector.GetGuid(i).ToString());
                        break;
                    case PhysicalType.Timestamp:
                        values.Add(vector.GetTimestamp(i).ToString("o"));
                        break;
                    default:
                        values.Add($"<{vector.TypeOfValue}>");
                        break;
                }
            }
            catch
            {
                values.Add("<error>");
            }
        }

        string result = $"[{string.Join(", ", values)}";
        if (vector.Size > printSize)
        {
            result += $", ... ({vector.Size - printSize} more)]";
        }
        else
        {
            result += "]";
        }
        
        return result;
    }

    private static void SearchForValueReal8(CollectionElement collection, string path)
    {
        foreach (Element element in collection.Elements)
        {
            string elementPath = $"{path}/{ElementTagShort(element.TagOfElement)}";

            if (element is ScalarElement scalar)
            {
                if (scalar.TypeOfValue == PhysicalType.Real8)
                {
                    double v = scalar.GetReal8();
                    if (Math.Abs(v - TARGET) < EPS)
                    {
                        Console.WriteLine($"✓ Found REAL8 ~ {TARGET} at {elementPath}");
                        Console.WriteLine($"  Tag: {scalar.TagOfElement}");
                        Console.WriteLine($"  Value: {v:G17}");
                    }
                }
            }
            else if (element is VectorElement vector)
            {
                if (vector.TypeOfValue == PhysicalType.Real8)
                {
                    for (int i = 0; i < vector.Size; i++)
                    {
                        double v = vector.GetReal8(i);
                        if (Math.Abs(v - TARGET) < EPS)
                        {
                            Console.WriteLine($"✓ Found REAL8 ~ {TARGET} in vector at {elementPath}[{i}]");
                            Console.WriteLine($"  Tag: {vector.TagOfElement}");
                            Console.WriteLine($"  Value: {v:G17}");
                        }
                    }
                }
            }
            else if (element is CollectionElement col)
            {
                SearchForValueReal8(col, elementPath);
            }
        }
    }

    private static string ElementTagShort(Guid tag)
    {
        // short form so paths aren't unreadable; returns last 8 chars of GUID
        string s = tag.ToString("N");
        return s.Length >= 8 ? s.Substring(s.Length - 8) : s;
    }
    
    private static string ElementTagInfo(Guid tag)
    {
        return $"Tag: {tag} (short: {ElementTagShort(tag)})";
    }
}