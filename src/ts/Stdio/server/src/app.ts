import { McpServer, ResourceTemplate } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { z } from "zod";
import EmployeeVacationService from "./services/employeeVacationService";

require('dotenv').config();

// Create an MCP server
const server = new McpServer({
  name: "Vacation Days Employees MCP",
  version: "1.0.0"
});

const connectionString = process.env.TABLE_CONNECTION_STRING || '"UseDevelopmentStorage=true"';
const tableName = process.env.TABLE_NAME || 'EmployeeVacationTable';

// Instantiate EmployeeVacationService
const employeeVacationService = new EmployeeVacationService(connectionString, tableName);

// Ensure the table is created and check if the table is empty, seed it with data if necessary
(async () => {
  await employeeVacationService.createTableIfNotExists();
  const isTableEmpty = await employeeVacationService.isTableEmpty();
  if (isTableEmpty) {
    console.log("Table is empty. Seeding data...");
    await employeeVacationService.seedFakeEmployees();
    console.log("Data seeding completed.");
  } else {
    console.log("Table already contains data.");
  }
})();

// Add an addition tool
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

// Start receiving messages on stdin and sending messages on stdout
const transport = new StdioServerTransport();
server.connect(transport);