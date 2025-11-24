using System;
using System.Collections.Generic;

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
