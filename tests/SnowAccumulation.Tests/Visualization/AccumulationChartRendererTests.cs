using SnowAccumulation.Core.Models;
using SnowAccumulation.Core.Visualization;

namespace SnowAccumulation.Tests.Visualization;

public class AccumulationChartRendererTests
{
    #region BuildTimeBuckets tests

    [Fact]
    public void BuildTimeBuckets_GroupsInto3HourBuckets()
    {
        var hourlyData = CreateHourlyData(hours: 24, snowCmPerHour: 1.0);

        var buckets = AccumulationChartRenderer.BuildTimeBuckets(hourlyData, useMetric: true);

        Assert.Equal(8, buckets.Count); // 24 hours / 3 = 8 buckets
    }

    [Fact]
    public void BuildTimeBuckets_SumsSnowWithinBucket_Metric()
    {
        var hourlyData = CreateHourlyData(hours: 3, snowCmPerHour: 2.0);

        var buckets = AccumulationChartRenderer.BuildTimeBuckets(hourlyData, useMetric: true);

        Assert.Single(buckets);
        Assert.Equal(6.0, buckets[0].Snow, precision: 1); // 3 × 2.0 cm = 6.0 cm
    }

    [Fact]
    public void BuildTimeBuckets_SumsSnowWithinBucket_Imperial()
    {
        var hourlyData = CreateHourlyData(hours: 3, snowCmPerHour: 2.54); // 2.54 cm ≈ 1 inch

        var buckets = AccumulationChartRenderer.BuildTimeBuckets(hourlyData, useMetric: false);

        Assert.Single(buckets);
        // 3 × 2.54 cm × 0.3937 ≈ 3.0 inches
        Assert.Equal(3.0, buckets[0].Snow, precision: 0);
    }

    [Fact]
    public void BuildTimeBuckets_HandlesPartialLastBucket()
    {
        var hourlyData = CreateHourlyData(hours: 5, snowCmPerHour: 1.0);

        var buckets = AccumulationChartRenderer.BuildTimeBuckets(hourlyData, useMetric: true);

        Assert.Equal(2, buckets.Count); // 3 + 2
        Assert.Equal(3.0, buckets[0].Snow); // first 3 hours
        Assert.Equal(2.0, buckets[1].Snow); // last 2 hours
    }

    [Fact]
    public void BuildTimeBuckets_AllZeroSnow_ReturnsZeroBuckets()
    {
        var hourlyData = CreateHourlyData(hours: 6, snowCmPerHour: 0.0);

        var buckets = AccumulationChartRenderer.BuildTimeBuckets(hourlyData, useMetric: true);

        Assert.Equal(2, buckets.Count);
        Assert.All(buckets, b => Assert.Equal(0.0, b.Snow));
    }

    [Fact]
    public void BuildTimeBuckets_EmptyInput_ReturnsEmpty()
    {
        var buckets = AccumulationChartRenderer.BuildTimeBuckets([], useMetric: true);

        Assert.Empty(buckets);
    }

    [Fact]
    public void BuildTimeBuckets_SetsLabelsCorrectly()
    {
        var hourlyData = CreateHourlyData(hours: 3, snowCmPerHour: 1.0);

        var buckets = AccumulationChartRenderer.BuildTimeBuckets(hourlyData, useMetric: true);

        // Label should contain the start time
        Assert.Contains("Feb 21", buckets[0].Label);
        Assert.Contains("00:00", buckets[0].Label);
    }

    [Fact]
    public void BuildTimeBuckets_SetsStartAndEndTimes()
    {
        var hourlyData = CreateHourlyData(hours: 6, snowCmPerHour: 1.0);

        var buckets = AccumulationChartRenderer.BuildTimeBuckets(hourlyData, useMetric: true);

        Assert.Equal(new DateTime(2026, 2, 21, 0, 0, 0), buckets[0].StartTime);
        Assert.Equal(new DateTime(2026, 2, 21, 3, 0, 0), buckets[0].EndTime);
        Assert.Equal(new DateTime(2026, 2, 21, 3, 0, 0), buckets[1].StartTime);
        Assert.Equal(new DateTime(2026, 2, 21, 6, 0, 0), buckets[1].EndTime);
    }

    #endregion

    #region Helpers

    private static List<HourlySnowData> CreateHourlyData(int hours, double snowCmPerHour)
    {
        var data = new List<HourlySnowData>();
        var startTime = new DateTime(2026, 2, 21, 0, 0, 0);

        for (int i = 0; i < hours; i++)
        {
            data.Add(new HourlySnowData
            {
                Time = startTime.AddHours(i),
                SnowCm = snowCmPerHour,
                TemperatureCelsius = -5.0,
                Condition = snowCmPerHour > 0 ? "Snow" : "Clear",
                ChanceOfSnow = snowCmPerHour > 0 ? 80 : 0,
                WindKph = 10.0,
                WindMph = 6.2
            });
        }

        return data;
    }

    #endregion
}
