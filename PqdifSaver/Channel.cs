using System;
using System.Runtime.CompilerServices;
using Gemstone.PQDIF.Logical;

namespace PQDIF_Manager
{
    public class Channel
    {
        public ChannelInstance ChannelInstance { get; private set; }
        public Phase Phase { get; private set; }
        public string? ChannelName { get; private set; }
        public int SeriesCount { get; private set; }
        public QuantityMeasured QuantityMeasured { get; private set; }
        public Series TimeSeries { get; private set; }
        public Series[] ValueSeries { get; private set; }

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