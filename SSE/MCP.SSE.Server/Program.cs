using MCP.SSE.Server.Services;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol.Messages;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Server;
using ModelContextProtocol.Utils.Json;
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
    .WithTools<EmployeeVacationTool>();

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

// MapAbsoluteEndpointUriMcp(app);

app.MapMcp();

app.Run();


static void MapAbsoluteEndpointUriMcp(IEndpointRouteBuilder endpoints)
{
    var loggerFactory = endpoints.ServiceProvider.GetRequiredService<ILoggerFactory>();
    var options = endpoints.ServiceProvider.GetRequiredService<IOptions<McpServerOptions>>().Value;
    var routeGroup = endpoints.MapGroup("");
    SseResponseStreamTransport? session = null;

    routeGroup.MapGet("/sse", async context =>
    {
        context.Response.Headers.ContentType = "text/event-stream";

        // Construct the absolute base URI dynamically.
        // var host = $"{context.Request.Scheme}://{context.Request.Host}";
        var host = $"https://qfpn28w9-5248.euw.devtunnels.ms";
        var transport = new SseResponseStreamTransport(context.Response.Body, $"{host}/message");
        session = transport;
        try
        {
            await using (transport)
            {
                var transportTask = transport.RunAsync(context.RequestAborted);
                await using var server = McpServerFactory.Create(transport, options, loggerFactory, endpoints.ServiceProvider);

                try
                {
                    await server.RunAsync(context.RequestAborted);
                }
                catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
                {
                    // Normal SSE disconnect.
                }
                catch (Exception ex)
                {
                    // Handle other exceptions as needed.
                    Log.Error(ex, "Error in SSE transport: {Message}", ex.Message);
                }

                await transportTask;
            }
        }
        catch (Exception ex)
        {

        }
    });

    routeGroup.MapPost("/message", async context =>
    {
        if (session is null)
        {
            await Results.BadRequest("Session not started.").ExecuteAsync(context);
            return;
        }

        var message = await context.Request.ReadFromJsonAsync<IJsonRpcMessage>(
            McpJsonUtilities.DefaultOptions, context.RequestAborted);
        if (message is null)
        {
            await Results.BadRequest("No message in request body.").ExecuteAsync(context);
            return;
        }

        await session.OnMessageReceivedAsync(message, context.RequestAborted);
        context.Response.StatusCode = StatusCodes.Status202Accepted;
        await context.Response.WriteAsync("Accepted");
    });
}