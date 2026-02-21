## Plan: Snow Accumulation CLI App

A .NET 9 C# console application that estimates snow accumulation at a given address (or auto-detected location) using **WeatherAPI.com**, renders a visual comparison of snow depth against a 6-foot person using **Spectre.Console**, and charts accumulation rate over time. Tests via **xUnit**.

WeatherAPI.com is the single external API — it provides hourly snow data (`snow_cm`), accepts addresses/zip codes directly as query parameters, and supports IP-based geolocation via `q=auto:ip`, eliminating the need for separate geocoding or IP services.

**Steps**

1. **Create solution structure** with three projects:
   - `src/SnowAccumulation.Console` — .NET 9 console app (entry point, CLI commands)
   - `src/SnowAccumulation.Core` — .NET 9 class library (services, models, visualization logic)
   - `tests/SnowAccumulation.Tests` — xUnit test project
   - `SnowAccumulation.sln` at the workspace root linking all three

2. **Add NuGet packages:**
   - Console project: `Spectre.Console`, `Spectre.Console.Cli`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.Configuration.Json`, `Microsoft.Extensions.Configuration.UserSecrets`
   - Core project: `Spectre.Console` (for rendering classes), `Polly` (HTTP resilience)
   - Test project: `xunit`, `xunit.runner.visualstudio`, `Moq`, `Microsoft.NET.Test.Sdk`

3. **Define models in `SnowAccumulation.Core/Models/`:**
   - `Location.cs` — lat/lon, city, region, country
   - `HourlySnowData.cs` — timestamp, snow amount (cm/inches), temperature, conditions text
   - `SnowForecast.cs` — location, list of `HourlySnowData`, total accumulation, forecast date range

4. **Implement `IWeatherService` in `SnowAccumulation.Core/Services/`:**
   - `IWeatherService.cs` — interface with `Task<SnowForecast> GetForecastByAddressAsync(string address)` and `Task<SnowForecast> GetForecastByAutoIpAsync()`
   - `WeatherApiService.cs` — calls WeatherAPI.com's `/forecast.json` endpoint with `days=3` and hourly data. Parses `hour.snow_cm` from each hour's response. Uses `HttpClient` via DI. Handles `q=auto:ip` for auto-detection, and `q={address}` for explicit addresses.
   - Include error handling for: invalid addresses (API 400), no API key, network failures. Use Polly retry policy for transient HTTP errors.

5. **Implement `ILocationService` in `SnowAccumulation.Core/Services/`:**
   - Wraps the location resolution aspect — formats/validates user input, passes to weather service
   - Detects whether user wants auto-detect vs. manual address

6. **Implement visualization in `SnowAccumulation.Core/Visualization/`:**
   - `SnowPersonRenderer.cs` — uses Spectre.Console `Canvas` to draw a side-by-side comparison:
     - Left side: 6-foot stick person (scaled to terminal height, e.g., 24 rows = 6 feet → 4 rows per foot)
     - Right side: snow level filled up to the forecasted depth, with depth label in inches/cm
     - Handle edge cases: 0 snow ("No snow expected!"), snow > 6 feet (person fully buried, show overflow amount)
   - `AccumulationChartRenderer.cs` — uses Spectre.Console `BarChart` or a custom canvas to show hourly/3-hourly accumulation rate over the forecast period. Each bar represents a time window with the snowfall amount. Include a running total line or cumulative label.

7. **Implement CLI entry point in `SnowAccumulation.Console/`:**
   - `Program.cs` — set up DI container, read API key from user secrets / environment variable `WEATHERAPI_KEY` / `appsettings.json` (in that priority order)
   - Use `Spectre.Console.Cli` with a default command that:
     - Prompts: "Enter an address (or press Enter to auto-detect your location)"
     - Calls weather service with the appropriate method
     - Displays: location resolved, total snow accumulation, the person-vs-snow graphic, the accumulation-over-time chart
   - Add `--address` / `-a` option for non-interactive use
   - Add `--units` / `-u` option for imperial (inches, default) vs. metric (cm)

8. **Configure API key management:**
   - `appsettings.json` with a placeholder `WeatherApiKey` field
   - Enable .NET User Secrets for the console project (for dev)
   - Also read from `WEATHERAPI_KEY` environment variable (for CI/production)
   - On missing key, display a helpful error explaining how to set it up

9. **Write unit tests in `SnowAccumulation.Tests/`:**
   - `WeatherApiServiceTests.cs` — mock `HttpMessageHandler` to return sample WeatherAPI.com JSON; verify parsing of snow data, handling of zero-snow responses, handling of API errors (400, 401, 500)
   - `LocationServiceTests.cs` — test input validation, address formatting
   - `SnowPersonRendererTests.cs` — test scaling logic: 0 inches, 12 inches, 72 inches (6 ft), >72 inches; verify row calculations are correct
   - `AccumulationChartRendererTests.cs` — test data aggregation (hourly → display buckets), handling of all-zero data
   - Include sample API response JSON as embedded test fixtures

**Verification**
- Run `dotnet build` to confirm all projects compile
- Run `dotnet test` to confirm all xUnit tests pass
- Manual test: run the app with a known snowy location (e.g., `--address "Buffalo, NY"`) and verify the person graphic and chart render correctly
- Manual test: run with no address to verify IP auto-detection works
- Manual test: run with a warm-climate address to verify the "no snow expected" message

**Decisions**
- **WeatherAPI.com** as the sole API — eliminates need for separate geocoding/IP services
- **.NET 9** as target framework per user preference
- **Spectre.Console** for all terminal visualization (person graphic via Canvas, accumulation chart via BarChart)
- **Interface-based services** with DI to keep Core library fully unit-testable without real API calls
- **Imperial (inches) as default** with metric option, since WeatherAPI returns cm and the problem describes a "6-foot person"
