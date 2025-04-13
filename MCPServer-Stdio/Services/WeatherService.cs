using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

public class WeatherService : IWeatherService
{
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(ILogger<WeatherService> logger)
    {
        _logger = logger;
    }

    public async Task<WeatherData> GetWeatherAsync(double latitude, double longitude)
    {
        HttpClient client = new HttpClient();

        // Ensure latitude and longitude use dot as decimal separator
        var finalLatitude = latitude.ToString().Replace(",", ".");
        var finalLongitude = longitude.ToString().Replace(",", ".");

        var dir = Directory.GetCurrentDirectory();
        _logger.LogInformation($"Current directory: {dir}");

        string url = $"https://api.open-meteo.com/v1/forecast?latitude={finalLatitude}&longitude={finalLongitude}&current_weather=true";
        _logger.LogInformation($"Requesting weather data from URL: {url}");

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        _logger.LogInformation($"Received response: {json}");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var weatherData = JsonSerializer.Deserialize<WeatherData>(json, options);
        if (weatherData == null)
        {
            throw new InvalidOperationException("Failed to deserialize weather data.");
        }
        return weatherData;
    }
}

