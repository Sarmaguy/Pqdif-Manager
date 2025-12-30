using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Gemstone.PQDIF.Logical;

namespace PQDIF_Manager
{
    /// <summary>
    /// Represents a parsed PQDIF file, including metadata, channels, and observation records.
    /// Provides async loading and measurement extraction utilities.
    /// </summary>
    public class PqdifFile
    {
        /// <summary>
        /// Gets the file path of the PQDIF file.
        /// </summary>
        public string FilePath { get; private set; }
        /// <summary>
        /// Gets the file creation time.
        /// </summary>
        public DateTime CreateTime { get; private set; }
        /// <summary>
        /// Gets the start time of the measurement.
        /// </summary>
        public DateTime StartTime { get; private set; }
        /// <summary>
        /// Gets the effective time of the data source, if available.
        /// </summary>
        public DateTime? EffectiveTime { get; private set; }
        /// <summary>
        /// Gets the name of the PQDIF file.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the device name associated with the file.
        /// </summary>
        public string DeviceName { get; private set; }
        /// <summary>
        /// Gets the data source location.
        /// </summary>
        public string DataSourceLocation { get; private set; }
        /// <summary>
        /// Gets the array of measurement channels in the file.
        /// </summary>
        public Channel[] Channels { get; private set; }
        /// <summary>
        /// Gets the observation record for the file.
        /// </summary>
        public ObservationRecord ObservationRecord { get; private set; }
        /// <summary>
        /// Gets the recording identifier for the file.
        /// </summary>
        public string RecordingId {get; private set;}
        /// <summary>
        /// Gets the UTC start timestamp for the measurement.
        /// </summary>
        public DateTime StartTimestampUtc => StartTime.ToUniversalTime();

        /// <summary>
        /// Initializes a new PqdifFile with all metadata and parsed channel/observation data.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="createTime">File creation time.</param>
        /// <param name="startTime">Measurement start time.</param>
        /// <param name="effectiveTime">Effective time of the data source.</param>
        /// <param name="name">File name.</param>
        /// <param name="deviceName">Device name.</param>
        /// <param name="dataSourceLocation">Data source location.</param>
        /// <param name="channels">Array of parsed channels.</param>
        /// <param name="observationRecord">Observation record.</param>
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

        /// <summary>
        /// Asynchronously loads and parses a PQDIF file from disk, extracting all channels and metadata.
        /// </summary>
        /// <param name="filePath">The path to the PQDIF file.</param>
        /// <returns>A parsed PqdifFile instance.</returns>
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



        /// <summary>
        /// Parses all measurements from the PQDIF file, returning a collection of Measurement objects.
        /// </summary>
        /// <returns>Enumerable of parsed Measurement objects.</returns>
        internal async Task<IEnumerable<Measurement>> ParseMeasurementsFromFile()
        {
            List<Measurement> measurements = new();

            //ISeriesInfoRepository seriesInfoSaver = new SqlServerSeriesInfoRepository();

            foreach (Channel channel in Channels)
            {
                Series timeSeries = channel.TimeSeries;
                for (int i = 0; i < channel.ValueSeries.Length; i++)
                {
                    Series valueSeries = channel.ValueSeries[i];

/*                     int seriesId = await seriesInfoSaver.GetSeriesIdAsync(
                        channel.ChannelName,
                        channel.QuantityMeasured.ToString(),
                        channel.Phase.ToString(),
                        valueSeries.SeriesValueType); */

                    /* if (seriesId == 0)
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
                    } */

                    for (int j = 0; j < timeSeries.SampleCount; j++)
                    {
                        DateTime timestampInUTC = StartTimestampUtc.AddSeconds((double)timeSeries.OriginalValues[j]);
                        double value = Convert.ToDouble(valueSeries.OriginalValues[j]);
                        measurements.Add(new Measurement
                        {
                            RecordingId = RecordingId,
                            timestamp = timestampInUTC,
                            Value = value,
                            SeriesId = 1
                        });
                    }
                }
            }
            return measurements;
        }
    }
}