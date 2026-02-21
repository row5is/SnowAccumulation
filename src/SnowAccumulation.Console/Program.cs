using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using SnowAccumulation.Console.Commands;
using SnowAccumulation.Core.Services;

// ── Configuration ──────────────────────────────────────────────────────────
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddUserSecrets<TypeRegistrar>(optional: true)
    .AddEnvironmentVariables()
    .Build();

// Resolve API key: User Secrets / env var WEATHERAPI_KEY / appsettings.json
var apiKey = configuration["WeatherApi:ApiKey"];
if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_API_KEY_HERE")
{
    apiKey = Environment.GetEnvironmentVariable("WEATHERAPI_KEY");
}

if (string.IsNullOrWhiteSpace(apiKey))
{
    AnsiConsole.MarkupLine("[red bold]No WeatherAPI key found![/]");
    AnsiConsole.MarkupLine("");
    AnsiConsole.MarkupLine("[yellow]Set your API key using one of these methods:[/]");
    AnsiConsole.MarkupLine("  1. Environment variable: [green]set WEATHERAPI_KEY=your_key[/]");
    AnsiConsole.MarkupLine("  2. .NET User Secrets:    [green]dotnet user-secrets set \"WeatherApi:ApiKey\" \"your_key\"[/]");
    AnsiConsole.MarkupLine("  3. appsettings.json:     [green]edit src/SnowAccumulation.Console/appsettings.json[/]");
    AnsiConsole.MarkupLine("");
    AnsiConsole.MarkupLine("Sign up for a free key at [link]https://www.weatherapi.com/signup.aspx[/]");
    return 1;
}

// ── Dependency Injection ───────────────────────────────────────────────────
var services = new ServiceCollection();

services.AddHttpClient<IWeatherService, WeatherApiService>(client =>
{
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(15);
}).ConfigureHttpClient((_, _) => { });

// Register WeatherApiService with the API key
services.AddSingleton<IWeatherService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = factory.CreateClient(nameof(IWeatherService));
    return new WeatherApiService(httpClient, apiKey);
});

services.AddTransient<ILocationService, LocationService>();

var registrar = new TypeRegistrar(services);

// ── CLI App ────────────────────────────────────────────────────────────────
var app = new CommandApp<ForecastCommand>(registrar);
app.Configure(config =>
{
    config.SetApplicationName("snowforecast");
    config.SetApplicationVersion("1.0.0");
});

return await app.RunAsync(args);

// ── Spectre.Console.Cli DI Registrar ──────────────────────────────────────

/// <summary>
/// Bridges Microsoft.Extensions.DependencyInjection with Spectre.Console.Cli.
/// </summary>
internal sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _services;

    public TypeRegistrar(IServiceCollection services)
    {
        _services = services;
    }

    public ITypeResolver Build() => new TypeResolver(_services.BuildServiceProvider());

    public void Register(Type service, Type implementation) =>
        _services.AddSingleton(service, implementation);

    public void RegisterInstance(Type service, object implementation) =>
        _services.AddSingleton(service, implementation);

    public void RegisterLazy(Type service, Func<object> factory) =>
        _services.AddSingleton(service, _ => factory());
}

internal sealed class TypeResolver : ITypeResolver
{
    private readonly IServiceProvider _provider;

    public TypeResolver(IServiceProvider provider)
    {
        _provider = provider;
    }

    public object? Resolve(Type? type) =>
        type is null ? null : _provider.GetService(type);
}
