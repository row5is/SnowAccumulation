using System.Net.Http.Json;
using System.Text.Json;
using SnowAccumulation.Core.Models;
using SnowAccumulation.Core.Services.Dto;

namespace SnowAccumulation.Core.Services;

/// <summary>
/// Implementation of <see cref="IWeatherService"/> using WeatherAPI.com.
/// </summary>
public class WeatherApiService : IWeatherService
{
    private const string BaseUrl = "https://api.weatherapi.com/v1";
    private const int ForecastDays = 3;

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    /// <summary>
    /// Initializes a new instance of <see cref="WeatherApiService"/>.
    /// </summary>
    /// <param name="httpClient">An <see cref="HttpClient"/> instance (typically injected via DI / IHttpClientFactory).</param>
    /// <param name="apiKey">A valid WeatherAPI.com API key.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKey"/> is null or empty.</exception>
    public WeatherApiService(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("WeatherAPI key must not be empty.", nameof(apiKey));

        _apiKey = apiKey;
    }

    /// <inheritdoc />
    public Task<SnowForecast> GetForecastByAddressAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address must not be empty.", nameof(address));

        return GetForecastAsync(address);
    }

    /// <inheritdoc />
    public Task<SnowForecast> GetForecastByAutoIpAsync()
    {
        return GetForecastAsync("auto:ip");
    }

    /// <summary>
    /// Calls the WeatherAPI.com forecast endpoint and maps the response to a <see cref="SnowForecast"/>.
    /// </summary>
    private async Task<SnowForecast> GetForecastAsync(string query)
    {
        var url = $"{BaseUrl}/forecast.json?key={Uri.EscapeDataString(_apiKey)}&q={Uri.EscapeDataString(query)}&days={ForecastDays}";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            WeatherApiErrorResponse? apiError = null;

            try
            {
                apiError = JsonSerializer.Deserialize<WeatherApiErrorResponse>(errorBody);
            }
            catch (JsonException)
            {
                // Could not parse error body — fall through to generic message.
            }

            var message = apiError?.Error.Message ?? $"WeatherAPI returned HTTP {(int)response.StatusCode}.";
            throw new WeatherApiException(message, (int)response.StatusCode, apiError?.Error.Code ?? 0);
        }

        var apiResponse = await response.Content.ReadFromJsonAsync<WeatherApiResponse>()
            ?? throw new WeatherApiException("Failed to deserialize WeatherAPI response.", 0, 0);

        return MapToSnowForecast(apiResponse, query);
    }

    /// <summary>
    /// Maps the raw WeatherAPI.com response into our domain <see cref="SnowForecast"/> model.
    /// </summary>
    internal static SnowForecast MapToSnowForecast(WeatherApiResponse apiResponse, string query)
    {
        var location = new Location
        {
            Latitude = apiResponse.Location.Lat,
            Longitude = apiResponse.Location.Lon,
            City = apiResponse.Location.Name,
            Region = apiResponse.Location.Region,
            Country = apiResponse.Location.Country,
            Query = query
        };

        var hourlyData = new List<HourlySnowData>();

        foreach (var day in apiResponse.Forecast.ForecastDay)
        {
            foreach (var hour in day.Hour)
            {
                hourlyData.Add(new HourlySnowData
                {
                    Time = ParseWeatherApiTime(hour.Time),
                    SnowCm = hour.SnowCm,
                    TemperatureCelsius = hour.TempC,
                    Condition = hour.Condition.Text,
                    ChanceOfSnow = hour.ChanceOfSnow,
                    WindKph = hour.WindKph,
                    WindMph = hour.WindMph
                });
            }
        }

        var forecast = new SnowForecast
        {
            Location = location,
            HourlyData = hourlyData
        };

        if (hourlyData.Count > 0)
        {
            forecast.ForecastStart = hourlyData[0].Time;
            forecast.ForecastEnd = hourlyData[^1].Time;
        }

        return forecast;
    }

    /// <summary>
    /// Parses the "yyyy-MM-dd HH:mm" time format returned by WeatherAPI.com.
    /// </summary>
    internal static DateTime ParseWeatherApiTime(string time)
    {
        if (DateTime.TryParseExact(time, "yyyy-MM-dd HH:mm",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var result))
        {
            return result;
        }

        // Fallback: let the runtime parse it.
        return DateTime.Parse(time, System.Globalization.CultureInfo.InvariantCulture);
    }
}

/// <summary>
/// Exception thrown when the WeatherAPI.com service returns an error.
/// </summary>
public class WeatherApiException : Exception
{
    /// <summary>
    /// The HTTP status code returned by the API, or 0 if not applicable.
    /// </summary>
    public int HttpStatusCode { get; }

    /// <summary>
    /// The WeatherAPI.com-specific error code (e.g., 1006 = No location found), or 0 if not available.
    /// </summary>
    public int ApiErrorCode { get; }

    public WeatherApiException(string message, int httpStatusCode, int apiErrorCode)
        : base(message)
    {
        HttpStatusCode = httpStatusCode;
        ApiErrorCode = apiErrorCode;
    }
}
