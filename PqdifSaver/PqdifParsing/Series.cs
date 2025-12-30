using System;
using Gemstone.PQDIF.Logical;

namespace PQDIF_Manager
{
/// <summary>
/// Represents a single series of measurement data in a PQDIF file, including value type, units, and values.
/// Wraps a Gemstone PQDIF SeriesInstance.
/// </summary>
    public class Series
    {
        /// <summary>
        /// Gets the underlying Gemstone PQDIF SeriesInstance.
        /// </summary>
        public SeriesInstance SeriesInstance { get; private set; }
        /// <summary>
        /// Gets the value type of the series (e.g., "Time", "RMS").
        /// </summary>
        public string? SeriesValueType { get; private set; }
        /// <summary>
        /// Gets the number of samples in the series.
        /// </summary>
        public int SampleCount { get; private set; }
        /// <summary>
        /// Gets the original values for the series.
        /// </summary>
        public IList<object> OriginalValues { get; private set; }
        /// <summary>
        /// Gets the GUID for the quantity characteristic of the series.
        /// </summary>
        public Guid QuantityCharacteristicID { get; private set; }
        /// <summary>
        /// Gets or sets the quantity characteristic as a string.
        /// </summary>
        public string? QuantityCharacteristic { get;  set; }
        /// <summary>
        /// Gets the units for the series values.
        /// </summary>
        public QuantityUnits QuantityUnits { get; private set; }

        /// <summary>
        /// Initializes a new Series from a Gemstone PQDIF SeriesInstance.
        /// </summary>
        /// <param name="seriesInstance">The underlying SeriesInstance.</param>
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