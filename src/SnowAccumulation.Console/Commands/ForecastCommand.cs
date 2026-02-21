using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using SnowAccumulation.Core.Models;
using SnowAccumulation.Core.Services;
using SnowAccumulation.Core.Visualization;

namespace SnowAccumulation.Console.Commands;

/// <summary>
/// The default CLI command that fetches and displays snow accumulation data.
/// </summary>
public class ForecastCommand : AsyncCommand<ForecastCommand.Settings>
{
    private readonly ILocationService _locationService;
    private readonly SnowPersonRenderer _personRenderer;
    private readonly AccumulationChartRenderer _chartRenderer;

    public ForecastCommand(ILocationService locationService)
    {
        _locationService = locationService;
        _personRenderer = new SnowPersonRenderer();
        _chartRenderer = new AccumulationChartRenderer();
    }

    public sealed class Settings : CommandSettings
    {
        [CommandOption("-a|--address <ADDRESS>")]
        [Description("Address, city, zip code, or lat/lon to look up. Leave blank to auto-detect via IP.")]
        public string? Address { get; set; }

        [CommandOption("-u|--units <UNITS>")]
        [Description("Unit system: 'imperial' (inches, default) or 'metric' (cm).")]
        [DefaultValue("imperial")]
        public string Units { get; set; } = "imperial";
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var console = AnsiConsole.Console;
        var useMetric = settings.Units.Equals("metric", StringComparison.OrdinalIgnoreCase);
        string? address = settings.Address;

        // If no address provided via --address, prompt interactively
        if (string.IsNullOrWhiteSpace(address))
        {
            var prompt = new TextPrompt<string>("[green]Enter an address, city, or zip code[/] [grey](or press Enter to auto-detect):[/] ")
                .AllowEmpty();
            address = console.Prompt(prompt);
        }

        SnowForecast forecast;

        try
        {
            await console.Status()
                .Spinner(Spinner.Known.Weather)
                .StartAsync("Fetching snow forecast...", async _ =>
                {
                    // Do nothing here — we need the result outside.
                    await Task.CompletedTask;
                });

            // Actual fetch
            forecast = await _locationService.GetForecastAsync(address);
        }
        catch (WeatherApiException ex)
        {
            console.MarkupLine($"[red]Error from weather service:[/] {Markup.Escape(ex.Message)}");
            if (ex.ApiErrorCode == 1006)
                console.MarkupLine("[yellow]Tip: Check the address or try a different format (e.g., city name, zip code, lat/lon).[/]");
            return 1;
        }
        catch (HttpRequestException ex)
        {
            console.MarkupLine($"[red]Network error:[/] {Markup.Escape(ex.Message)}");
            console.MarkupLine("[yellow]Check your internet connection and try again.[/]");
            return 1;
        }

        // Display location
        console.WriteLine();
        console.Write(new Rule($"[bold]Snow Forecast for {Markup.Escape(forecast.Location.DisplayName)}[/]").RuleStyle("blue"));
        console.WriteLine();

        var loc = forecast.Location;
        console.MarkupLine($"[grey]Location:[/] {Markup.Escape(loc.DisplayName)} ({loc.Latitude:F2}, {loc.Longitude:F2})");
        console.MarkupLine($"[grey]Forecast:[/] {forecast.ForecastStart:MMM dd} — {forecast.ForecastEnd:MMM dd, yyyy}");
        console.WriteLine();

        // Render the person-vs-snow graphic
        _personRenderer.Render(console, forecast, useMetric);

        // Render the accumulation-over-time chart
        _chartRenderer.Render(console, forecast, useMetric);

        return 0;
    }
}
