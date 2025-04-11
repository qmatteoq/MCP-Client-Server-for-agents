using System.ComponentModel;
using ModelContextProtocol.Server;

[McpServerToolType]
public static class EchoTool
{
    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo(string message) => $"Hello from C#: {message}";

    [McpServerTool, Description("Echoes in reverse the message sent by the client.")]
    public static string ReverseEcho(string message) => new string(message.Reverse().ToArray());
}

[McpServerToolType]
public static class WeatherTool
{
    [McpServerTool, Description("Get the current weather data for a given latitude and longitude.")]
    public static async Task<WeatherData> GetWeatherAsync(IWeatherService weatherService, [Description("The latitude of the location")] double latitude, [Description("The longitude of the location")] double longitude)
    {
        return await weatherService.GetWeatherAsync(latitude, longitude);
    }
}

[McpServerToolType]
public static class MathTool
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
