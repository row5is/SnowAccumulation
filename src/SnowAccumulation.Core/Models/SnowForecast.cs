namespace SnowAccumulation.Core.Models;

/// <summary>
/// Represents a complete snow accumulation forecast for a location.
/// </summary>
public class SnowForecast
{
    /// <summary>
    /// The resolved location for this forecast.
    /// </summary>
    public Location Location { get; set; } = new();

    /// <summary>
    /// Hourly snow data points across the forecast period.
    /// </summary>
    public List<HourlySnowData> HourlyData { get; set; } = [];

    /// <summary>
    /// The start of the forecast period.
    /// </summary>
    public DateTime ForecastStart { get; set; }

    /// <summary>
    /// The end of the forecast period.
    /// </summary>
    public DateTime ForecastEnd { get; set; }

    /// <summary>
    /// Total snow accumulation in centimeters across the entire forecast period.
    /// </summary>
    public double TotalSnowCm => HourlyData.Sum(h => h.SnowCm);

    /// <summary>
    /// Total snow accumulation in inches across the entire forecast period.
    /// </summary>
    public double TotalSnowInches => TotalSnowCm * 0.3937;

    /// <summary>
    /// Total snow accumulation in feet.
    /// </summary>
    public double TotalSnowFeet => TotalSnowInches / 12.0;

    /// <summary>
    /// Whether any snow is expected during the forecast period.
    /// </summary>
    public bool HasSnow => TotalSnowCm > 0;

    /// <summary>
    /// Gets the hourly data grouped by day.
    /// </summary>
    public IEnumerable<IGrouping<DateOnly, HourlySnowData>> ByDay =>
        HourlyData.GroupBy(h => DateOnly.FromDateTime(h.Time));

    /// <summary>
    /// Gets a running cumulative total of snow in centimeters at each hour.
    /// </summary>
    public IEnumerable<(DateTime Time, double CumulativeCm)> CumulativeSnow()
    {
        double running = 0;
        foreach (var hour in HourlyData)
        {
            running += hour.SnowCm;
            yield return (hour.Time, running);
        }
    }
}
