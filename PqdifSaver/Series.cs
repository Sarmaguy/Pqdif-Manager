using System;
using Gemstone.PQDIF.Logical;

namespace PQDIF_Manager
{
    public class Series
    {
        public SeriesInstance SeriesInstance { get; private set; }
        public string? SeriesValueType { get; private set; }
        public int SampleCount { get; private set; }
        public IList<object> OriginalValues { get; private set; }
        public Guid QuantityCharacteristicID { get; private set; }
        public string? QuantityCharacteristic { get;  set; }
        public QuantityUnits QuantityUnits { get; private set; }

        public Series(Gemstone.PQDIF.Logical.SeriesInstance seriesInstance)
        {
            this.SeriesInstance = seriesInstance;
            SeriesDefinition sDefinition = seriesInstance.Definition;
            SeriesValueType = Gemstone.PQDIF.Logical.SeriesValueType.ToString(sDefinition.ValueTypeID);
            OriginalValues = seriesInstance.OriginalValues;
            SampleCount = OriginalValues.Count;
            QuantityCharacteristicID = sDefinition.QuantityCharacteristicID;
            QuantityCharacteristic = Gemstone.PQDIF.Logical.QuantityCharacteristic.ToString(QuantityCharacteristicID);
            QuantityUnits = sDefinition.QuantityUnits;
        }

    }
}