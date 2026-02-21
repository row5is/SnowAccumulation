using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using SnowAccumulation.Core.Services;
using SnowAccumulation.Core.Services.Dto;

namespace SnowAccumulation.Tests.Services;

public class WeatherApiServiceTests
{
    private const string FakeApiKey = "test-api-key-123";

    #region MapToSnowForecast tests (static, no HTTP needed)

    [Fact]
    public void MapToSnowForecast_ParsesLocation_Correctly()
    {
        var json = TestFixtures.LoadJson("sample_forecast.json");
        var apiResponse = JsonSerializer.Deserialize<WeatherApiResponse>(json)!;

        var forecast = WeatherApiService.MapToSnowForecast(apiResponse, "Buffalo, NY");

        Assert.Equal("Buffalo", forecast.Location.City);
        Assert.Equal("New York", forecast.Location.Region);
        Assert.Equal("United States of America", forecast.Location.Country);
        Assert.Equal(42.89, forecast.Location.Latitude);
        Assert.Equal(-78.88, forecast.Location.Longitude);
        Assert.Equal("Buffalo, NY", forecast.Location.Query);
    }

    [Fact]
    public void MapToSnowForecast_ParsesHourlyData_WithCorrectCount()
    {
        var json = TestFixtures.LoadJson("sample_forecast.json");
        var apiResponse = JsonSerializer.Deserialize<WeatherApiResponse>(json)!;

        var forecast = WeatherApiService.MapToSnowForecast(apiResponse, "Buffalo, NY");

        Assert.Equal(24, forecast.HourlyData.Count); // 24 hours in the fixture
    }

    [Fact]
    public void MapToSnowForecast_CalculatesTotalSnow_Correctly()
    {
        var json = TestFixtures.LoadJson("sample_forecast.json");
        var apiResponse = JsonSerializer.Deserialize<WeatherApiResponse>(json)!;

        var forecast = WeatherApiService.MapToSnowForecast(apiResponse, "Buffalo, NY");

        // Sum of snow_cm in sample: 0+0+0.5+1+1.5+2+2+1.5+1.5+1+1+0.5+0.5+0.5+0.5+0.5+0.5+0.5+0+0+0+0+0+0 = 15.0
        Assert.Equal(15.0, forecast.TotalSnowCm, precision: 1);
        Assert.True(forecast.HasSnow);
    }

    [Fact]
    public void MapToSnowForecast_NoSnow_HasSnowIsFalse()
    {
        var json = TestFixtures.LoadJson("sample_forecast_no_snow.json");
        var apiResponse = JsonSerializer.Deserialize<WeatherApiResponse>(json)!;

        var forecast = WeatherApiService.MapToSnowForecast(apiResponse, "Miami, FL");

        Assert.Equal(0, forecast.TotalSnowCm);
        Assert.False(forecast.HasSnow);
    }

    [Fact]
    public void MapToSnowForecast_ParsesTime_Correctly()
    {
        var json = TestFixtures.LoadJson("sample_forecast.json");
        var apiResponse = JsonSerializer.Deserialize<WeatherApiResponse>(json)!;

        var forecast = WeatherApiService.MapToSnowForecast(apiResponse, "Buffalo, NY");

        var firstHour = forecast.HourlyData[0];
        Assert.Equal(new DateTime(2026, 2, 21, 0, 0, 0), firstHour.Time);
    }

    [Fact]
    public void MapToSnowForecast_ParsesSnowCmPerHour()
    {
        var json = TestFixtures.LoadJson("sample_forecast.json");
        var apiResponse = JsonSerializer.Deserialize<WeatherApiResponse>(json)!;

        var forecast = WeatherApiService.MapToSnowForecast(apiResponse, "Buffalo, NY");

        // Hour index 5 (05:00) should have 2.0 cm snow
        Assert.Equal(2.0, forecast.HourlyData[5].SnowCm);
        Assert.Equal("Heavy snow", forecast.HourlyData[5].Condition);
        Assert.Equal(85, forecast.HourlyData[5].ChanceOfSnow);
    }

    [Fact]
    public void MapToSnowForecast_SetsForecastDates()
    {
        var json = TestFixtures.LoadJson("sample_forecast.json");
        var apiResponse = JsonSerializer.Deserialize<WeatherApiResponse>(json)!;

        var forecast = WeatherApiService.MapToSnowForecast(apiResponse, "Buffalo, NY");

        Assert.Equal(new DateTime(2026, 2, 21, 0, 0, 0), forecast.ForecastStart);
        Assert.Equal(new DateTime(2026, 2, 21, 23, 0, 0), forecast.ForecastEnd);
    }

    #endregion

    #region ParseWeatherApiTime tests

    [Theory]
    [InlineData("2026-02-21 00:00", 2026, 2, 21, 0, 0)]
    [InlineData("2026-02-21 14:30", 2026, 2, 21, 14, 30)]
    [InlineData("2026-12-31 23:59", 2026, 12, 31, 23, 59)]
    public void ParseWeatherApiTime_ParsesCorrectly(string input, int year, int month, int day, int hour, int minute)
    {
        var result = WeatherApiService.ParseWeatherApiTime(input);

        Assert.Equal(new DateTime(year, month, day, hour, minute, 0), result);
    }

    #endregion

    #region HTTP integration tests with mocked handler

    [Fact]
    public async Task GetForecastByAddressAsync_ReturnsForcast_OnSuccess()
    {
        var json = TestFixtures.LoadJson("sample_forecast.json");
        var handler = CreateMockHandler(HttpStatusCode.OK, json);
        var httpClient = new HttpClient(handler.Object);
        var service = new WeatherApiService(httpClient, FakeApiKey);

        var forecast = await service.GetForecastByAddressAsync("Buffalo, NY");

        Assert.NotNull(forecast);
        Assert.Equal("Buffalo", forecast.Location.City);
        Assert.True(forecast.HasSnow);
    }

    [Fact]
    public async Task GetForecastByAutoIpAsync_ReturnsForcast_OnSuccess()
    {
        var json = TestFixtures.LoadJson("sample_forecast.json");
        var handler = CreateMockHandler(HttpStatusCode.OK, json);
        var httpClient = new HttpClient(handler.Object);
        var service = new WeatherApiService(httpClient, FakeApiKey);

        var forecast = await service.GetForecastByAutoIpAsync();

        Assert.NotNull(forecast);
    }

    [Fact]
    public async Task GetForecastByAddressAsync_ThrowsWeatherApiException_On400()
    {
        var errorJson = """{"error":{"code":1006,"message":"No matching location found."}}""";
        var handler = CreateMockHandler(HttpStatusCode.BadRequest, errorJson);
        var httpClient = new HttpClient(handler.Object);
        var service = new WeatherApiService(httpClient, FakeApiKey);

        var ex = await Assert.ThrowsAsync<WeatherApiException>(
            () => service.GetForecastByAddressAsync("xyznonexistent"));

        Assert.Equal(400, ex.HttpStatusCode);
        Assert.Equal(1006, ex.ApiErrorCode);
        Assert.Contains("No matching location found", ex.Message);
    }

    [Fact]
    public async Task GetForecastByAddressAsync_ThrowsWeatherApiException_On401()
    {
        var errorJson = """{"error":{"code":2006,"message":"API key provided is invalid."}}""";
        var handler = CreateMockHandler(HttpStatusCode.Unauthorized, errorJson);
        var httpClient = new HttpClient(handler.Object);
        var service = new WeatherApiService(httpClient, FakeApiKey);

        var ex = await Assert.ThrowsAsync<WeatherApiException>(
            () => service.GetForecastByAddressAsync("Buffalo, NY"));

        Assert.Equal(401, ex.HttpStatusCode);
        Assert.Equal(2006, ex.ApiErrorCode);
    }

    [Fact]
    public async Task GetForecastByAddressAsync_ThrowsWeatherApiException_OnServerError()
    {
        var handler = CreateMockHandler(HttpStatusCode.InternalServerError, "Internal Server Error");
        var httpClient = new HttpClient(handler.Object);
        var service = new WeatherApiService(httpClient, FakeApiKey);

        var ex = await Assert.ThrowsAsync<WeatherApiException>(
            () => service.GetForecastByAddressAsync("Buffalo, NY"));

        Assert.Equal(500, ex.HttpStatusCode);
    }

    [Fact]
    public void Constructor_ThrowsOnEmptyApiKey()
    {
        var httpClient = new HttpClient();
        Assert.Throws<ArgumentException>(() => new WeatherApiService(httpClient, ""));
        Assert.Throws<ArgumentException>(() => new WeatherApiService(httpClient, "   "));
    }

    [Fact]
    public async Task GetForecastByAddressAsync_ThrowsOnEmptyAddress()
    {
        var httpClient = new HttpClient();
        var service = new WeatherApiService(httpClient, FakeApiKey);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GetForecastByAddressAsync(""));
    }

    #endregion

    #region Helpers

    private static Mock<HttpMessageHandler> CreateMockHandler(HttpStatusCode statusCode, string content)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
            });
        return handler;
    }

    #endregion
}
