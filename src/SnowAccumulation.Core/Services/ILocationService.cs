using SnowAccumulation.Core.Models;

namespace SnowAccumulation.Core.Services;

/// <summary>
/// Service for resolving a user's location and retrieving the snow forecast.
/// Wraps <see cref="IWeatherService"/> with input handling and validation.
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Resolves the user's intent (explicit address or auto-detect) and returns a snow forecast.
    /// </summary>
    /// <param name="userInput">
    /// The user-provided address, zip code, city, or lat/lon.
    /// If null or empty, auto-detection via IP geolocation is used.
    /// </param>
    /// <returns>A <see cref="SnowForecast"/> for the resolved location.</returns>
    Task<SnowForecast> GetForecastAsync(string? userInput);

    /// <summary>
    /// Determines whether the given input represents an auto-detect request
    /// (null, empty, whitespace, or explicitly "auto:ip").
    /// </summary>
    bool IsAutoDetect(string? userInput);

    /// <summary>
    /// Normalizes and trims user input for use as a weather query.
    /// Returns null if the input should trigger auto-detection.
    /// </summary>
    string? NormalizeInput(string? userInput);
}
