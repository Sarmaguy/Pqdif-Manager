using System;
using System.Collections.Generic;

/// <summary>
/// Provides static methods for converting phase identifiers to standardized column names for voltage and current.
/// </summary>
public class PhaseConverter
{
    private static readonly Dictionary<string, string> voltageMap = new Dictionary<string, string>
    {
        { "AB", "U12" },
        { "BC", "U23" },
        { "CA", "U31" },
        { "AN", "U1" },
        { "BN", "U2" },
        { "CN", "U3" },
        { "N",  "UN" } 
    };

    private static readonly Dictionary<string, string> currentMap = new Dictionary<string, string>
    {
        { "AB", "I12" },
        { "BC", "I23" },
        { "CA", "I31" },
        { "AN", "I1" },
        { "BN", "I2" },
        { "CN", "I3" },
        { "N",  "IN" } 
    };

    /// <summary>
    /// Converts a phase identifier and type (voltage/current) to a standardized column name.
    /// </summary>
    /// <param name="phase">The phase identifier (e.g., "AN", "AB").</param>
    /// <param name="type">The type of measurement ("voltage" or "current").</param>
    /// <returns>The standardized column name.</returns>
    /// <exception cref="ArgumentException">Thrown if the type or phase is not recognized.</exception>
    public static string ConvertPhase(string phase, string type)
    {
        if (type.ToLower() == "voltage")
        {
            if (voltageMap.ContainsKey(phase))
                return voltageMap[phase];
        }
        else if (type.ToLower() == "current")
        {
            if (currentMap.ContainsKey(phase))
                return currentMap[phase];
        }
        else
        {
            throw new ArgumentException("Type must be 'voltage' or 'current'");
        }

        throw new ArgumentException($"Phase '{phase}' is not recognized");
    }
    
}
