"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const express_1 = __importDefault(require("express"));
const mcp_js_1 = require("@modelcontextprotocol/sdk/server/mcp.js");
const streamableHttp_js_1 = require("@modelcontextprotocol/sdk/server/streamableHttp.js");
const zod_1 = require("zod");
const employeeVacationService_js_1 = __importDefault(require("./services/employeeVacationService.js"));
require('dotenv').config();
const connectionString = process.env.TABLE_CONNECTION_STRING || '"UseDevelopmentStorage=true"';
const tableName = process.env.TABLE_NAME || 'EmployeeVacationTable';
// Instantiate EmployeeVacationService
const employeeVacationService = new employeeVacationService_js_1.default(connectionString, tableName);
const getServer = () => {
    // Create an MCP server with implementation details
    const server = new mcp_js_1.McpServer({
        name: 'stateless-streamable-http-server',
        version: '1.0.0',
    }, { capabilities: { logging: {} } });
    server.tool("getVacationDaysLeftAsync", "Get the vacation days left for a given employee", {
        name: zod_1.z.string().describe("The name of the employee")
    }, async ({ name }) => ({
        content: [{ type: "text", text: String(await employeeVacationService.getVacationDaysLeft(name)) }]
    }));
    server.tool("chargeVacationDaysAsync", "Charge vacation days for a given employee.", {
        name: zod_1.z.string().describe("The name of the employee"),
        daysToCharge: zod_1.z.number().describe("The number of vacation days to charge")
    }, async ({ name, daysToCharge }) => ({
        content: [{ type: "text", text: String(await employeeVacationService.chargeVacationDays(name, daysToCharge)) }]
    }));
    server.tool("getAllEmployeesAsync", "Get the list of employees with their number of vacation days left", {}, async () => {
        const response = await employeeVacationService.getAllEmployees();
        return {
            content: [{ type: "text", text: JSON.stringify(response) }]
        };
    });
    return server;
};
const app = (0, express_1.default)();
app.use(express_1.default.json());
app.post('/mcp', async (req, res) => {
    const server = getServer();
    try {
        const transport = new streamableHttp_js_1.StreamableHTTPServerTransport({
            sessionIdGenerator: undefined,
        });
        await server.connect(transport);
        await transport.handleRequest(req, res, req.body);
        res.on('close', () => {
            console.log('Request closed');
            transport.close();
            server.close();
        });
    }
    catch (error) {
        console.error('Error handling MCP request:', error);
        if (!res.headersSent) {
            res.status(500).json({
                jsonrpc: '2.0',
                error: {
                    code: -32603,
                    message: 'Internal server error',
                },
                id: null,
            });
        }
    }
});
app.get('/mcp', async (req, res) => {
    console.log('Received GET MCP request');
    res.writeHead(405).end(JSON.stringify({
        jsonrpc: "2.0",
        error: {
            code: -32000,
            message: "Method not allowed."
        },
        id: null
    }));
});
app.delete('/mcp', async (req, res) => {
    console.log('Received DELETE MCP request');
    res.writeHead(405).end(JSON.stringify({
        jsonrpc: "2.0",
        error: {
            code: -32000,
            message: "Method not allowed."
        },
        id: null
    }));
});
// Start the server
const PORT = 3000;
app.listen(PORT, async () => {
    await employeeVacationService.createTableIfNotExists();
    const isTableEmpty = await employeeVacationService.isTableEmpty();
    if (isTableEmpty) {
        console.log("Table is empty. Seeding data...");
        await employeeVacationService.seedFakeEmployees();
        console.log("Data seeding completed.");
    }
    else {
        console.log("Table already contains data.");
    }
    console.log(`MCP Stateless Streamable HTTP Server listening on port ${PORT}`);
});
// Handle server shutdown
process.on('SIGINT', async () => {
    console.log('Shutting down server...');
    process.exit(0);
});
