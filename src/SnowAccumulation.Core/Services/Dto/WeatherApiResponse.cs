using System.Text.Json.Serialization;

namespace SnowAccumulation.Core.Services.Dto;

/// <summary>
/// Root response from WeatherAPI.com /forecast.json endpoint.
/// </summary>
public class WeatherApiResponse
{
    [JsonPropertyName("location")]
    public WeatherApiLocation Location { get; set; } = new();

    [JsonPropertyName("forecast")]
    public WeatherApiForecast Forecast { get; set; } = new();
}

public class WeatherApiLocation
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("region")]
    public string Region { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }

    [JsonPropertyName("tz_id")]
    public string TzId { get; set; } = string.Empty;

    [JsonPropertyName("localtime")]
    public string LocalTime { get; set; } = string.Empty;
}

public class WeatherApiForecast
{
    [JsonPropertyName("forecastday")]
    public List<WeatherApiForecastDay> ForecastDay { get; set; } = [];
}

public class WeatherApiForecastDay
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("date_epoch")]
    public long DateEpoch { get; set; }

    [JsonPropertyName("day")]
    public WeatherApiDay Day { get; set; } = new();

    [JsonPropertyName("hour")]
    public List<WeatherApiHour> Hour { get; set; } = [];
}

public class WeatherApiDay
{
    [JsonPropertyName("maxtemp_c")]
    public double MaxTempC { get; set; }

    [JsonPropertyName("maxtemp_f")]
    public double MaxTempF { get; set; }

    [JsonPropertyName("mintemp_c")]
    public double MinTempC { get; set; }

    [JsonPropertyName("mintemp_f")]
    public double MinTempF { get; set; }

    [JsonPropertyName("totalsnow_cm")]
    public double TotalSnowCm { get; set; }

    [JsonPropertyName("daily_will_it_snow")]
    public int DailyWillItSnow { get; set; }

    [JsonPropertyName("daily_chance_of_snow")]
    public int DailyChanceOfSnow { get; set; }

    [JsonPropertyName("condition")]
    public WeatherApiCondition Condition { get; set; } = new();
}

public class WeatherApiHour
{
    [JsonPropertyName("time_epoch")]
    public long TimeEpoch { get; set; }

    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;

    [JsonPropertyName("temp_c")]
    public double TempC { get; set; }

    [JsonPropertyName("temp_f")]
    public double TempF { get; set; }

    [JsonPropertyName("snow_cm")]
    public double SnowCm { get; set; }

    [JsonPropertyName("will_it_snow")]
    public int WillItSnow { get; set; }

    [JsonPropertyName("chance_of_snow")]
    public int ChanceOfSnow { get; set; }

    [JsonPropertyName("wind_mph")]
    public double WindMph { get; set; }

    [JsonPropertyName("wind_kph")]
    public double WindKph { get; set; }

    [JsonPropertyName("condition")]
    public WeatherApiCondition Condition { get; set; } = new();
}

public class WeatherApiCondition
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public int Code { get; set; }
}

/// <summary>
/// Error response from WeatherAPI.com.
/// </summary>
public class WeatherApiErrorResponse
{
    [JsonPropertyName("error")]
    public WeatherApiError Error { get; set; } = new();
}

public class WeatherApiError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
