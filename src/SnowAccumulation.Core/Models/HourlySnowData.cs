namespace SnowAccumulation.Core.Models;

/// <summary>
/// Represents snow and weather data for a single hour.
/// </summary>
public class HourlySnowData
{
    /// <summary>
    /// The date and time for this data point.
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// Snowfall amount in centimeters for this hour.
    /// </summary>
    public double SnowCm { get; set; }

    /// <summary>
    /// Snowfall amount in inches for this hour.
    /// Calculated from <see cref="SnowCm"/> using 1 cm ≈ 0.3937 inches.
    /// </summary>
    public double SnowInches => SnowCm * 0.3937;

    /// <summary>
    /// Temperature in Celsius.
    /// </summary>
    public double TemperatureCelsius { get; set; }

    /// <summary>
    /// Temperature in Fahrenheit.
    /// </summary>
    public double TemperatureFahrenheit => (TemperatureCelsius * 9.0 / 5.0) + 32.0;

    /// <summary>
    /// Weather condition description (e.g., "Heavy snow", "Light snow", "Clear").
    /// </summary>
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// Chance of snow as a percentage (0-100).
    /// </summary>
    public int ChanceOfSnow { get; set; }

    /// <summary>
    /// Wind speed in kilometers per hour.
    /// </summary>
    public double WindKph { get; set; }

    /// <summary>
    /// Wind speed in miles per hour.
    /// </summary>
    public double WindMph { get; set; }
}
