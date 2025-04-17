using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var openai = builder.AddConnectionString("openAiConnectionName");
var tableService = builder.AddConnectionString("tableServiceConnectionName");

var mcpServer = builder.AddProject<MCP_Server_Web>("mcp-server")
.WithReference(tableService);

builder.AddProject<MCP_Client_SK>("semantic-kernel-client")
.WithReference(mcpServer)
.WithReference(openai);

builder.Build().Run();
