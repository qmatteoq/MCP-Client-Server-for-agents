using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var openai = builder.AddConnectionString("openAiConnectionName");
var tableService = builder.AddConnectionString("tableServiceConnectionName");

var mcpServer = builder.AddProject<MCP_SSE_Server>("MCP-SSE-Server")
.WithReference(tableService);

builder.AddProject<MCP_SSE_Client>("MCP-SSE-Client")
.WithReference(mcpServer)
.WithReference(openai);

builder.Build().Run();
