using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Gemstone.PQDIF.Logical;

namespace PQDIF_Manager
{
    public class PqdifFile
    {
        public string FilePath { get; private set; }
        public DateTime CreateTime { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime? EffectiveTime { get; private set; }
        public string Name { get; private set; }
        public string DeviceName { get; private set; }
        public string DataSourceLocation { get; private set; }
        public Channel[] Channels { get; private set; }
        public ObservationRecord ObservationRecord { get; private set; }
        public string RecordingId {get; private set;}
        public DateTime StartTimestampUtc => StartTime.ToUniversalTime();

        private PqdifFile(string filePath, DateTime createTime, DateTime startTime, DateTime effectiveTime,
        string name, string deviceName, string dataSourceLocation, Channel[] channels, ObservationRecord observationRecord)
        {

            FilePath = filePath;
            CreateTime = createTime;
            StartTime = startTime;
            EffectiveTime = effectiveTime;
            Name = name;
            DeviceName = deviceName;
            DataSourceLocation = dataSourceLocation;
            Channels = channels;
            ObservationRecord = observationRecord;
            RecordingId = observationRecord.DataSource.DataSourceSerialNumber;

        }

        public static async Task<PqdifFile> LoadFromFileAsync(string filePath)
        {

            List<ObservationRecord> observationRecords = new();

            await using (LogicalParser parser = new LogicalParser(filePath))
            {
                await parser.OpenAsync();

                while (await parser.HasNextObservationRecordAsync())
                    observationRecords.Add(await parser.NextObservationRecordAsync());
            }

            if (observationRecords.Count == 0)
                throw new Exception("The PQDIF file contains no observation records.");

            ObservationRecord observation = observationRecords[0];
            string deviceName = observation.DataSource?.DataSourceName ?? "(unknown)";

            return new(
                filePath,
                observation.CreateTime,
                observation.StartTime,
                observation.DataSource.Effective,
                observation.Name,
                deviceName,
                observation.DataSource?.DataSourceLocation ?? "(unknown)",
                observation.ChannelInstances.Select(ci => new Channel(ci)).ToArray(),
                observation
            );
        }



        internal async Task<IEnumerable<Measurement>> ParseMeasurementsFromFile()
        {
            List<Measurement> measurements = new();

            ISeriesInfoRepository seriesInfoSaver = new SqlServerSeriesInfoRepository();

            foreach (Channel channel in Channels)
            {
                Series timeSeries = channel.TimeSeries;
                for (int i = 0; i < channel.ValueSeries.Length; i++)
                {
                    Series valueSeries = channel.ValueSeries[i];

                    int seriesId = await seriesInfoSaver.GetSeriesIdAsync(
                        channel.ChannelName,
                        channel.QuantityMeasured.ToString(),
                        channel.Phase.ToString(),
                        valueSeries.SeriesValueType);

                    if (seriesId == 0)
                    {
                        SeriesInfo seriesInfo = new SeriesInfo
                        {
                            ChannelName = channel.ChannelName,
                            QuantityMeasured = channel.QuantityMeasured.ToString(),
                            Phase = channel.Phase.ToString(),
                            SeriesValueType = valueSeries.SeriesValueType
                        };

                        await seriesInfoSaver.SaveSeriesInfoAsync(seriesInfo);
                        seriesId = seriesInfo.SeriesId;
                    }

                    for (int j = 0; j < timeSeries.SampleCount; j++)
                    {
                        DateTime timestampInUTC = StartTimestampUtc.AddSeconds((double)timeSeries.OriginalValues[j]);
                        double value = Convert.ToDouble(valueSeries.OriginalValues[j]);
                        measurements.Add(new Measurement
                        {
                            RecordingId = RecordingId,
                            timestamp = timestampInUTC,
                            Value = value,
                            SeriesId = seriesId
                        });
                    }
                }
            }
            return measurements;
        }
    }
}