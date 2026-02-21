using System.Reflection;

namespace SnowAccumulation.Tests;

/// <summary>
/// Helper to load embedded JSON test fixture files.
/// </summary>
internal static class TestFixtures
{
    /// <summary>
    /// Reads an embedded JSON fixture file by short name (e.g., "sample_forecast.json").
    /// </summary>
    public static string LoadJson(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
            ?? throw new FileNotFoundException($"Embedded resource '{fileName}' not found. Available: {string.Join(", ", assembly.GetManifestResourceNames())}");

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
