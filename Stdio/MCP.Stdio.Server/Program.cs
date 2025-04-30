using MCP.Stdio.Server.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(standardErrorFromLevel: Serilog.Events.LogEventLevel.Information)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

var table = builder.Configuration.GetValue<string>("ConnectionStrings:tableServiceConnectionName");

builder.Services
    .AddSingleton<IEmployeeVacationService>(sp =>
    {
        var connectionString = builder.Configuration.GetValue<string>("ConnectionStrings:tableServiceConnectionName");
        var tableName = "VacationsTable";
        return new EmployeeVacationService(connectionString, tableName, sp.GetRequiredService<ILogger<EmployeeVacationService>>());
    });

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var vacationService = scope.ServiceProvider.GetRequiredService<IEmployeeVacationService>() as EmployeeVacationService;
    if (vacationService != null && await vacationService.IsTableEmptyAsync())
    {
        await vacationService.SeedFakeEmployeesAsync();
    }
}

await app.RunAsync();