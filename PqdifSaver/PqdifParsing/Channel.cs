using System;
using System.Runtime.CompilerServices;
using Gemstone.PQDIF.Logical;

namespace PQDIF_Manager
{
    /// <summary>
    /// Represents a measurement channel in a PQDIF file, including phase, name, series, and measured quantity.
    /// Handles time/value series ordering and provides access to all series data.
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// Gets the underlying ChannelInstance from Gemstone PQDIF.
        /// </summary>
        public ChannelInstance ChannelInstance { get; private set; }
        /// <summary>
        /// Gets or sets the phase for this channel.
        /// </summary>
        public Phase Phase { get;  set; }
        /// <summary>
        /// Gets the channel name.
        /// </summary>
        public string? ChannelName { get; private set; }
        /// <summary>
        /// Gets the number of series in this channel.
        /// </summary>
        public int SeriesCount { get; private set; }
        /// <summary>
        /// Gets the measured quantity for this channel.
        /// </summary>
        public QuantityMeasured QuantityMeasured { get; private set; }
        /// <summary>
        /// Gets the time series for this channel.
        /// </summary>
        public Series TimeSeries { get; private set; }
        /// <summary>
        /// Gets the value series for this channel (all series except time).
        /// </summary>
        public Series[] ValueSeries { get; private set; }

        /// <summary>
        /// Initializes a new Channel from a ChannelInstance, handling time/value series order.
        /// </summary>
        /// <param name="channelInstance">The underlying ChannelInstance.</param>
        public Channel(ChannelInstance channelInstance)
        {
            this.ChannelInstance = channelInstance;
            ChannelDefinition cDefinition = channelInstance.Definition;
            Phase = cDefinition.Phase;
            ChannelName = cDefinition.ChannelName;
            SeriesCount = channelInstance.SeriesInstances.Count;
            QuantityMeasured = cDefinition.QuantityMeasured;

            //The IEEE 1159.3 standard does not strictly require the first series to be time.
            if (channelInstance.SeriesInstances[0].Definition.ValueTypeID != Gemstone.PQDIF.Logical.SeriesValueType.Time)
            {
                FixSeriesOrder(); return;
            }

            TimeSeries = new Series(channelInstance.SeriesInstances[0]);
            ValueSeries = new Series[SeriesCount - 1];
            for (int i = 1; i < SeriesCount; i++)
            {
                ValueSeries[i - 1] = new Series(channelInstance.SeriesInstances[i]);
            }
        }

        /// <summary>
        /// Ensures the time series is first and value series are ordered correctly.
        /// </summary>
        private void FixSeriesOrder()
        {
            //Find the time series
            int timeIndex = -1;
            for (int i = 0; i < SeriesCount; i++)
            {
                if (ChannelInstance.SeriesInstances[i].Definition.ValueTypeID == Gemstone.PQDIF.Logical.SeriesValueType.Time)
                {
                    timeIndex = i;
                    break;
                }
            }

            if (timeIndex == -1)
                throw new Exception($"Channel {ChannelName} is missing a time series.");

            TimeSeries = new Series(ChannelInstance.SeriesInstances[timeIndex]);
            ValueSeries = new Series[SeriesCount - 1];
            int valueSeriesIndex = 0;
            for (int i = 0; i < SeriesCount; i++)
            {
                if (i == timeIndex)  continue;

                ValueSeries[valueSeriesIndex] = new Series(ChannelInstance.SeriesInstances[i]);
                valueSeriesIndex++;
            }
        }
    }
}