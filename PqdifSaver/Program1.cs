using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Gemstone.PQDIF.Logical;
namespace TestBench
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string filePath = @"C:\Users\Jura\Desktop\PSL_P3003845_2025-04-12_10Min_ClassA_PQDIF.pqd";
            string outputFile = Path.ChangeExtension(filePath, ".txt"); 

            List<ObservationRecord> observationRecords = new();


            await using (LogicalParser parser = new LogicalParser(filePath))
            {
                await parser.OpenAsync();

                while (await parser.HasNextObservationRecordAsync())
                    observationRecords.Add(await parser.NextObservationRecordAsync());
            }

            if (observationRecords.Count == 0)
            {
                await File.WriteAllTextAsync(outputFile, "No observation records found.");
                return;
            }

            ObservationRecord observation = observationRecords[0];
            string deviceName = observation.DataSource?.DataSourceName ?? "(unknown)";


            await using StreamWriter writer = new StreamWriter(outputFile);


            await writer.WriteLineAsync($"Device: {deviceName}");
            await writer.WriteLineAsync($"Channels: {observation.ChannelInstances.Count}");
            await writer.WriteLineAsync();

            foreach (ChannelInstance channel in observation.ChannelInstances)
            {
                ChannelDefinition def = channel.Definition;


                await writer.WriteLineAsync($"Channel: {def.ChannelName,-10} | Phase={def.Phase,-4} | Quantity={def.QuantityMeasured}");
                await writer.WriteLineAsync($"  Series count: {channel.SeriesInstances.Count}");
                await writer.WriteLineAsync($"Quantity Type: {def.QuantityTypeID.ToString()}");
                await writer.WriteLineAsync($"Quantity info: {QuantityType.ToString(def.QuantityTypeID)}");

                foreach (SeriesInstance sInst in channel.SeriesInstances)
                {
                    SeriesDefinition sDef = sInst.Definition;
                    
                    
                    string QC = QuantityCharacteristic.ToString(sDef.QuantityCharacteristicID);
                    Guid tagValueTypeID = sDef.ValueTypeID;
                    string units = sDef.QuantityUnits.ToString();
                    string storageMethodID = sDef.StorageMethodID.ToString();
                    await writer.WriteLineAsync($"    Value Type: {SeriesValueType.ToString(tagValueTypeID)} | Units={units,-8} | storageMethodID={storageMethodID} | QuantityCharateristic={sDef.QuantityCharacteristicID.ToString()} | ValueType={sDef.ValueTypeID.ToString()}");

                    var data = sInst.OriginalValues;

                    if (data != null)
                    {
                        int count = data.Count;
                        await writer.WriteLineAsync($"      Number of values: {count}");

                        foreach (var value in data)
                        {
                            await writer.WriteLineAsync($"        Value = {value}");
                            break;
                        }
                    }

                    await writer.WriteLineAsync();
                    
                }

            await writer.WriteLineAsync(new string('-', 60));
            }

            Console.WriteLine($"Data written to: {outputFile}");
        }
    }
}
