using MCP.SSE.Server.Services;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(), "weather.log"), rollingInterval: RollingInterval.Day)
    .WriteTo.Debug()
    .WriteTo.Console(standardErrorFromLevel: Serilog.Events.LogEventLevel.Information)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

var connectionString = builder.Configuration.GetValue<string>("ConnectionStrings:tableServiceConnectionName");

builder.Services
    .AddSingleton<IEmployeeVacationService>(sp =>
    {
        var connectionString = builder.Configuration.GetValue<string>("ConnectionStrings:tableServiceConnectionName");
        var tableName = "VacationsTable";
        return new EmployeeVacationService(connectionString, tableName, sp.GetRequiredService<ILogger<EmployeeVacationService>>());
    })
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

builder.Services.AddOpenTelemetry()
    .WithTracing(b => b.AddSource("*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithMetrics(b => b.AddMeter("*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithLogging()
    .UseOtlpExporter();

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