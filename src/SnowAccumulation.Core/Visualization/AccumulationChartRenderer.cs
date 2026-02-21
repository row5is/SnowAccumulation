using Spectre.Console;
using SnowAccumulation.Core.Models;

namespace SnowAccumulation.Core.Visualization;

/// <summary>
/// Renders a chart showing the rate of snow accumulation over time using Spectre.Console.
/// </summary>
public class AccumulationChartRenderer
{
    /// <summary>
    /// Renders the accumulation-over-time display to the given console.
    /// Shows both a per-period bar chart and a cumulative summary table.
    /// </summary>
    public void Render(IAnsiConsole console, SnowForecast forecast, bool useMetric = false)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(forecast);

        if (!forecast.HasSnow)
        {
            return; // Nothing to chart; the person renderer already shows "no snow" message.
        }

        console.MarkupLine("[bold blue]Snow Accumulation Over Time[/]");
        console.WriteLine();

        // Aggregate hourly data into 3-hour buckets for a cleaner chart
        var buckets = BuildTimeBuckets(forecast.HourlyData, useMetric);

        if (buckets.Count == 0)
            return;

        // -- Bar Chart: snowfall per time window --
        var barChart = new BarChart()
            .Width(72)
            .Label("[bold]Snowfall per 3-hour period[/]");

        foreach (var bucket in buckets)
        {
            if (bucket.Snow > 0)
            {
                barChart.AddItem(bucket.Label, Math.Round(bucket.Snow, 1), Color.Blue);
            }
        }

        // Only render the bar chart if there are bars to show
        if (barChart.Data.Count > 0)
        {
            console.Write(barChart);
            console.WriteLine();
        }

        // -- Cumulative Table --
        RenderCumulativeTable(console, forecast, useMetric);
    }

    /// <summary>
    /// Renders a table showing cumulative snow totals by day.
    /// </summary>
    private static void RenderCumulativeTable(IAnsiConsole console, SnowForecast forecast, bool useMetric)
    {
        var unit = useMetric ? "cm" : "in";

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]Daily Snow Summary[/]")
            .AddColumn("Date")
            .AddColumn(new TableColumn($"Snowfall ({unit})").RightAligned())
            .AddColumn(new TableColumn($"Cumulative ({unit})").RightAligned())
            .AddColumn("Peak Hour");

        double cumulative = 0;

        foreach (var dayGroup in forecast.ByDay)
        {
            var date = dayGroup.Key;
            var hours = dayGroup.ToList();

            var daySnowCm = hours.Sum(h => h.SnowCm);
            var daySnow = useMetric ? daySnowCm : daySnowCm * 0.3937;
            cumulative += daySnow;

            var peakHour = hours.OrderByDescending(h => h.SnowCm).First();
            var peakLabel = peakHour.SnowCm > 0
                ? $"{peakHour.Time:HH:mm} ({(useMetric ? peakHour.SnowCm : peakHour.SnowInches):F1} {unit})"
                : "—";

            var snowColor = daySnow > 0 ? "blue" : "grey";

            table.AddRow(
                $"[bold]{date:ddd MMM dd}[/]",
                $"[{snowColor}]{daySnow:F1}[/{snowColor}]",
                $"[{snowColor}]{cumulative:F1}[/{snowColor}]",
                peakLabel
            );
        }

        console.Write(table);
        console.WriteLine();
    }

    /// <summary>
    /// Aggregates hourly snow data into 3-hour buckets.
    /// </summary>
    internal static List<TimeBucket> BuildTimeBuckets(List<HourlySnowData> hourlyData, bool useMetric)
    {
        var buckets = new List<TimeBucket>();

        for (int i = 0; i < hourlyData.Count; i += 3)
        {
            var chunk = hourlyData.Skip(i).Take(3).ToList();
            var startTime = chunk[0].Time;
            var endTime = chunk[^1].Time.AddHours(1);

            var snowCm = chunk.Sum(h => h.SnowCm);
            var snow = useMetric ? snowCm : snowCm * 0.3937;

            var label = $"{startTime:MMM dd HH:mm}";

            buckets.Add(new TimeBucket
            {
                StartTime = startTime,
                EndTime = endTime,
                Snow = snow,
                Label = label
            });
        }

        return buckets;
    }

    /// <summary>
    /// Represents a time window with aggregated snowfall.
    /// </summary>
    internal class TimeBucket
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Snow { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}
