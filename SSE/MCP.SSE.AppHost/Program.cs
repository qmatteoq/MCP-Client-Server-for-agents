using Aspire.Hosting;
using Microsoft.Extensions.Configuration;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var openai = builder.AddConnectionString("openAiConnectionName");
var tableService = builder.AddConnectionString("tableServiceConnectionName");

var mcpServer = builder.AddProject<MCP_SSE_Server>("MCP-SSE-Server")
.WithReference(tableService);

builder.AddProject<MCP_SSE_Client>("MCP-SSE-Client")
.WithReference(mcpServer)
.WithReference(openai);

var port = builder.Configuration.GetValue<int>("PORT");
var devToolsPort = port + 1;

builder.AddNpmApp("ttk2-agent", "../ttk2-agent", "dev")
.WithReference(mcpServer)
.WithEnvironment("DEV_TOOLS_PORT", devToolsPort.ToString())
.WithHttpEndpoint(name: "http", env: "PORT")
.WithHttpEndpoint(name: "devtools", env: "DEV_TOOLS_PORT")
.WithExternalHttpEndpoints();

builder.Build().Run();
