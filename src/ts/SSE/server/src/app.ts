import express, { Request, Response } from 'express';
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StreamableHTTPServerTransport } from "@modelcontextprotocol/sdk/server/streamableHttp.js";
import { z } from 'zod';
import { CallToolResult, GetPromptResult, ReadResourceResult } from "@modelcontextprotocol/sdk/types.js"
import EmployeeVacationService from './services/employeeVacationService.js';

const employeeVacationService = new EmployeeVacationService("UseDevelopmentStorage=true", "VacationsTable");



const getServer = (): McpServer => {
    // Create an MCP server with implementation details
    const server = new McpServer({
        name: 'stateless-streamable-http-server',
        version: '1.0.0',
    }, { capabilities: { logging: {} } });

    server.tool("getVacationDaysLeftAsync",
        "Get the vacation days left for a given employee",
        {
            name: z.string().describe("The name of the employee")
        },
        async ({ name }) => ({
            content: [{ type: "text", text: String(await employeeVacationService.getVacationDaysLeft(name)) }]
        })
    );

    server.tool("chargeVacationDaysAsync",
        "Charge vacation days for a given employee.",
        {
            name: z.string().describe("The name of the employee"),
            daysToCharge: z.number().describe("The number of vacation days to charge")
        },
        async ({ name, daysToCharge }) => ({
            content: [{ type: "text", text: String(await employeeVacationService.chargeVacationDays(name, daysToCharge)) }]
        })
    );

    server.tool(
        "getAllEmployeesAsync",
        "Get the list of employees with their number of vacation days left",
        {},
        async () => {
            const response = await employeeVacationService.getAllEmployees();
            return {
                content: [{ type: "text", text: JSON.stringify(response) }]
            };
        }
    );

    return server;
}

const app = express();
app.use(express.json());

app.post('/mcp', async (req: Request, res: Response) => {
    const server: McpServer = getServer();
    try {
        const transport: StreamableHTTPServerTransport = new StreamableHTTPServerTransport({
            sessionIdGenerator: undefined,
        });
        await server.connect(transport);
        await transport.handleRequest(req, res, req.body);
        res.on('close', () => {
            console.log('Request closed');
            transport.close();
            server.close();
        });
    } catch (error) {
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

app.get('/mcp', async (req: Request, res: Response) => {
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

app.delete('/mcp', async (req: Request, res: Response) => {
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
    } else {
        console.log("Table already contains data.");
    }
    console.log(`MCP Stateless Streamable HTTP Server listening on port ${PORT}`);
});

// Handle server shutdown
process.on('SIGINT', async () => {
    console.log('Shutting down server...');
    process.exit(0);
});
