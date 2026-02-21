using SnowAccumulation.Core.Models;

namespace SnowAccumulation.Core.Services;

/// <summary>
/// Service for retrieving snow accumulation forecast data.
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Gets a snow forecast for the specified address, city, or zip code.
    /// </summary>
    /// <param name="address">An address, city name, zip code, or lat/lon (e.g., "Buffalo, NY", "10001", "48.85,2.35").</param>
    /// <returns>A <see cref="SnowForecast"/> with hourly snow data.</returns>
    Task<SnowForecast> GetForecastByAddressAsync(string address);

    /// <summary>
    /// Gets a snow forecast by auto-detecting the user's location via IP geolocation.
    /// </summary>
    /// <returns>A <see cref="SnowForecast"/> with hourly snow data for the detected location.</returns>
    Task<SnowForecast> GetForecastByAutoIpAsync();
}
