using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(), "weather.log"), rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services
    .AddSingleton<IWeatherService, WeatherService>()
    .AddMcpServer().WithHttpTransport()
    .WithTools<EchoTool>()
    .WithTools<WeatherTool>();

var app = builder.Build();

app.MapMcp();

app.Run();