# Snow Accumulation CLI

A .NET 9 command-line application that estimates snow accumulation at a given address and displays a visual comparison of the forecasted snow depth against a 6-foot person, along with an accumulation-over-time chart.

Powered by [WeatherAPI.com](https://www.weatherapi.com/) and [Spectre.Console](https://spectreconsole.net/).

## Features

- **Snow forecast** — retrieves a 3-day hourly snow accumulation forecast for any address, city, zip code, or lat/lon coordinates
- **Auto-detect location** — if no address is provided, determines your location via IP geolocation
- **Person-vs-snow graphic** — ASCII visualization comparing forecasted snow depth to a 6-foot person
- **Accumulation chart** — bar chart showing snowfall rate in 3-hour intervals, plus a daily summary table with cumulative totals
- **Unit support** — imperial (inches, default) or metric (cm) output

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- A free [WeatherAPI.com](https://www.weatherapi.com/signup.aspx) API key (1M calls/month on the free tier)

## Setup

1. **Clone the repository**

   ```bash
   git clone https://github.com/row5is/SnowAccumulation.git
   cd SnowAccumulation
   ```

2. **Set your API key** (choose one method)

   **Option A — .NET User Secrets (recommended, keeps key out of source control):**

   ```bash
   cd src/SnowAccumulation.Console
   dotnet user-secrets set "WeatherApi:ApiKey" "YOUR_API_KEY"
   cd ../..
   ```

   **Option B — Environment variable:**

   ```bash
   # Windows
   set WEATHERAPI_KEY=YOUR_API_KEY

   # Linux/macOS
   export WEATHERAPI_KEY=YOUR_API_KEY
   ```

   **Option C — Edit appsettings.json** (not recommended for shared repos):

   Replace `YOUR_API_KEY_HERE` in `src/SnowAccumulation.Console/appsettings.json`.

3. **Build**

   ```bash
   dotnet build
   ```

## Usage

```bash
# Look up a specific address
dotnet run --project src/SnowAccumulation.Console -- --address "Buffalo, NY"

# Use a zip code
dotnet run --project src/SnowAccumulation.Console -- --address "14201"

# Auto-detect your location (via IP)
dotnet run --project src/SnowAccumulation.Console

# Use metric units (cm)
dotnet run --project src/SnowAccumulation.Console -- --address "Buffalo, NY" --units metric
```

### Command-line options

| Option | Description |
|---|---|
| `-a`, `--address` | Address, city, zip code, or lat/lon. Omit to auto-detect via IP. |
| `-u`, `--units` | `imperial` (inches, default) or `metric` (cm). |
| `--help` | Show help. |
| `--version` | Show version. |

If `--address` is not provided, the app will prompt you interactively. Press Enter at the prompt to use IP-based auto-detection.

## Project Structure

```
SnowAccumulation/
├── src/
│   ├── SnowAccumulation.Console/       # CLI entry point, DI setup, command
│   └── SnowAccumulation.Core/          # Core library (testable, no console dependency)
│       ├── Models/                      # SnowForecast, HourlySnowData, Location
│       ├── Services/                    # IWeatherService, WeatherApiService, LocationService
│       └── Visualization/              # SnowPersonRenderer, AccumulationChartRenderer
└── tests/
    └── SnowAccumulation.Tests/          # xUnit tests (67 tests)
```

## Running Tests

```bash
dotnet test
```

## License

MIT
