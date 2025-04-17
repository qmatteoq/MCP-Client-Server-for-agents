using MCPServer_Web.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(), "weather.log"), rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
builder.Configuration.AddEnvironmentVariables();

var connectionString = builder.Configuration.GetValue<string>("ConnectionStrings:tableServiceConnectionName");

builder.Services
    .AddSingleton<IWeatherService, WeatherService>()
    .AddSingleton<IEmployeeVacationService>(sp =>
    {
        var connectionString = builder.Configuration.GetValue<string>("ConnectionStrings:tableServiceConnectionName");
        var tableName = "VacationsTable";
        return new EmployeeVacationService(connectionString, tableName);
    })
    .AddMcpServer()
    .WithTools<WeatherTool>()
    .WithTools<EmployeeVacationTool>();

var app = builder.Build();

// Seed fake employees if table is empty
using (var scope = app.Services.CreateScope())
{
    var vacationService = scope.ServiceProvider.GetRequiredService<IEmployeeVacationService>() as EmployeeVacationService;
    if (vacationService != null && await vacationService.IsTableEmptyAsync())
    {
        await vacationService.SeedFakeEmployeesAsync();
    }
}

app.MapMcp();

app.Run();