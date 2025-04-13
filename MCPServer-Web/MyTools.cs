using System.ComponentModel;
using ModelContextProtocol.Server;
using Serilog;

[McpServerToolType]
public class EchoTool
{

    [McpServerTool, Description("Echoes the message back to the client.")]
    public string Echo(string message) => $"Hello from C#: {message}";

    [McpServerTool, Description("Echoes in reverse the message sent by the client.")]
    public string ReverseEcho(string message)  => new string(message.Reverse().ToArray());
}

[McpServerToolType]
public class WeatherTool
{
    private readonly IWeatherService _weatherService;
    public WeatherTool(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }


    [McpServerTool, Description("Get the current weather data for a given latitude and longitude.")]
    public async Task<WeatherData> GetWeatherAsync([Description("The latitude of the location")] double latitude, [Description("The longitude of the location")] double longitude)
    {
        return await _weatherService.GetWeatherAsync(latitude, longitude);
    }
}

[McpServerToolType]
public class MathTool
{
    [McpServerTool, Description("Adds two numbers together.")]
    public static int Add(int a, int b) => a + b;

    [McpServerTool, Description("Subtracts the second number from the first.")]
    public static int Subtract(int a, int b) => a - b;

    [McpServerTool, Description("Multiplies two numbers together.")]
    public static int Multiply(int a, int b) => a * b;

    [McpServerTool, Description("Divides the first number by the second.")]
    public static double Divide(int a, int b) => (double)a / b;
}
