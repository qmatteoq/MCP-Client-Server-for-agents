"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const mcp_js_1 = require("@modelcontextprotocol/sdk/server/mcp.js");
const stdio_js_1 = require("@modelcontextprotocol/sdk/server/stdio.js");
const zod_1 = require("zod");
const employeeVacationService_1 = __importDefault(require("./services/employeeVacationService"));
// Create an MCP server
const server = new mcp_js_1.McpServer({
    name: "Vacation Days Employees MCP",
    version: "1.0.0"
});
// Instantiate EmployeeVacationService
const employeeVacationService = new employeeVacationService_1.default("UseDevelopmentStorage=true", "VacationsTable");
// Ensure the table is created and check if the table is empty, seed it with data if necessary
(async () => {
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
})();
// Add an addition tool
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
// Start receiving messages on stdin and sending messages on stdout
const transport = new stdio_js_1.StdioServerTransport();
server.connect(transport);
