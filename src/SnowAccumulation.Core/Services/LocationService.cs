using SnowAccumulation.Core.Models;

namespace SnowAccumulation.Core.Services;

/// <summary>
/// Default implementation of <see cref="ILocationService"/>.
/// Delegates weather fetching to <see cref="IWeatherService"/> after normalizing user input.
/// </summary>
public class LocationService : ILocationService
{
    private readonly IWeatherService _weatherService;

    public LocationService(IWeatherService weatherService)
    {
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
    }

    /// <inheritdoc />
    public async Task<SnowForecast> GetForecastAsync(string? userInput)
    {
        var normalized = NormalizeInput(userInput);

        if (normalized is null)
        {
            return await _weatherService.GetForecastByAutoIpAsync();
        }

        return await _weatherService.GetForecastByAddressAsync(normalized);
    }

    /// <inheritdoc />
    public bool IsAutoDetect(string? userInput)
    {
        return NormalizeInput(userInput) is null;
    }

    /// <inheritdoc />
    public string? NormalizeInput(string? userInput)
    {
        if (string.IsNullOrWhiteSpace(userInput))
            return null;

        var trimmed = userInput.Trim();

        // Treat explicit "auto:ip" as auto-detect
        if (trimmed.Equals("auto:ip", StringComparison.OrdinalIgnoreCase))
            return null;

        return trimmed;
    }
}
