public interface ISeriesInfoRepository
{
    Task<int> GetSeriesIdAsync(string channelName, string quantityMeasured, string phase, string seriesValueType);
    Task SaveSeriesInfoAsync(SeriesInfo seriesInfo);
}