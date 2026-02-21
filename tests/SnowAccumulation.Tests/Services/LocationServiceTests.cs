using Moq;
using SnowAccumulation.Core.Models;
using SnowAccumulation.Core.Services;

namespace SnowAccumulation.Tests.Services;

public class LocationServiceTests
{
    #region NormalizeInput tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("auto:ip")]
    [InlineData("AUTO:IP")]
    [InlineData("  auto:ip  ")]
    public void NormalizeInput_ReturnsNull_ForAutoDetectInputs(string? input)
    {
        var service = CreateLocationService();
        Assert.Null(service.NormalizeInput(input));
    }

    [Theory]
    [InlineData("Buffalo, NY", "Buffalo, NY")]
    [InlineData("  Buffalo, NY  ", "Buffalo, NY")]
    [InlineData("10001", "10001")]
    [InlineData("48.85,2.35", "48.85,2.35")]
    public void NormalizeInput_TrimsAndReturns_ForValidAddresses(string input, string expected)
    {
        var service = CreateLocationService();
        Assert.Equal(expected, service.NormalizeInput(input));
    }

    #endregion

    #region IsAutoDetect tests

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("auto:ip", true)]
    [InlineData("Buffalo, NY", false)]
    [InlineData("10001", false)]
    public void IsAutoDetect_ReturnsCorrectResult(string? input, bool expected)
    {
        var service = CreateLocationService();
        Assert.Equal(expected, service.IsAutoDetect(input));
    }

    #endregion

    #region GetForecastAsync tests

    [Fact]
    public async Task GetForecastAsync_CallsAutoIp_WhenInputIsNull()
    {
        var mockWeather = new Mock<IWeatherService>();
        var expectedForecast = new SnowForecast { Location = new Location { City = "Detected City" } };
        mockWeather.Setup(w => w.GetForecastByAutoIpAsync()).ReturnsAsync(expectedForecast);

        var service = new LocationService(mockWeather.Object);
        var result = await service.GetForecastAsync(null);

        Assert.Equal("Detected City", result.Location.City);
        mockWeather.Verify(w => w.GetForecastByAutoIpAsync(), Times.Once);
        mockWeather.Verify(w => w.GetForecastByAddressAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetForecastAsync_CallsAutoIp_WhenInputIsEmpty()
    {
        var mockWeather = new Mock<IWeatherService>();
        var expectedForecast = new SnowForecast { Location = new Location { City = "Detected City" } };
        mockWeather.Setup(w => w.GetForecastByAutoIpAsync()).ReturnsAsync(expectedForecast);

        var service = new LocationService(mockWeather.Object);
        var result = await service.GetForecastAsync("");

        mockWeather.Verify(w => w.GetForecastByAutoIpAsync(), Times.Once);
    }

    [Fact]
    public async Task GetForecastAsync_CallsByAddress_WhenAddressProvided()
    {
        var mockWeather = new Mock<IWeatherService>();
        var expectedForecast = new SnowForecast { Location = new Location { City = "Buffalo" } };
        mockWeather.Setup(w => w.GetForecastByAddressAsync("Buffalo, NY")).ReturnsAsync(expectedForecast);

        var service = new LocationService(mockWeather.Object);
        var result = await service.GetForecastAsync("Buffalo, NY");

        Assert.Equal("Buffalo", result.Location.City);
        mockWeather.Verify(w => w.GetForecastByAddressAsync("Buffalo, NY"), Times.Once);
        mockWeather.Verify(w => w.GetForecastByAutoIpAsync(), Times.Never);
    }

    [Fact]
    public async Task GetForecastAsync_TrimsAddress_BeforeCalling()
    {
        var mockWeather = new Mock<IWeatherService>();
        var expectedForecast = new SnowForecast();
        mockWeather.Setup(w => w.GetForecastByAddressAsync("Buffalo, NY")).ReturnsAsync(expectedForecast);

        var service = new LocationService(mockWeather.Object);
        await service.GetForecastAsync("  Buffalo, NY  ");

        mockWeather.Verify(w => w.GetForecastByAddressAsync("Buffalo, NY"), Times.Once);
    }

    #endregion

    private static LocationService CreateLocationService()
    {
        return new LocationService(Mock.Of<IWeatherService>());
    }
}
