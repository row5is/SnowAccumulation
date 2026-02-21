namespace SnowAccumulation.Core.Models;

/// <summary>
/// Represents a geographic location resolved from an address or IP geolocation.
/// </summary>
public class Location
{
    /// <summary>
    /// Latitude coordinate.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude coordinate.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// City or locality name.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// State, province, or region name.
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Country name.
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// The original query used to resolve this location (address, zip code, or "auto:ip").
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Returns a human-readable display string for this location.
    /// </summary>
    public string DisplayName =>
        string.IsNullOrWhiteSpace(Region)
            ? $"{City}, {Country}"
            : $"{City}, {Region}, {Country}";
}
