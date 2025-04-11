using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Serilog;
using System.ComponentModel;
using System.IO;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(), "weather.log"), rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services
    .AddSingleton<IWeatherService, WeatherService>()
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();