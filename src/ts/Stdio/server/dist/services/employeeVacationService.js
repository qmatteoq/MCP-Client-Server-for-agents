"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const data_tables_1 = require("@azure/data-tables");
class EmployeeVacationService {
    constructor(connectionString, tableName) {
        this.tableClient = data_tables_1.TableClient.fromConnectionString(connectionString, tableName);
    }
    async getVacationDaysLeft(employeeName) {
        try {
            const entity = await this.tableClient.getEntity("Employee", employeeName);
            console.log(`Fetched vacation days left for ${employeeName}: ${entity.vacationDaysLeft}`);
            return entity.vacationDaysLeft;
        }
        catch (error) {
            if (error.statusCode === 404) {
                return null;
            }
            throw error;
        }
    }
    async chargeVacationDays(employeeName, daysToCharge) {
        try {
            const entityResult = await this.tableClient.getEntity("Employee", employeeName);
            const entity = entityResult;
            if (entity.vacationDaysLeft < daysToCharge) {
                return false;
            }
            entity.vacationDaysLeft -= daysToCharge;
            await this.tableClient.updateEntity(entity, "Merge");
            console.log(`Charged ${daysToCharge} vacation days to ${employeeName}. Remaining days: ${entity.vacationDaysLeft}`);
            return true;
        }
        catch (error) {
            if (error.statusCode === 404) {
                return false;
            }
            throw error;
        }
    }
    async isTableEmpty() {
        const entities = this.tableClient.listEntities();
        for await (const _ of entities) {
            console.log("Table is not empty.");
            return false;
        }
        console.log("Table is empty.");
        return true;
    }
    async seedFakeEmployees() {
        const random = Math.random;
        const names = [
            "Alice Johnson", "Bob Smith", "Charlie Lee", "Diana Evans", "Ethan Brown",
            "Fiona Clark", "George Miller", "Hannah Davis", "Ian Wilson", "Julia Adams"
        ];
        const tasks = names.map(name => {
            const employee = {
                partitionKey: "Employee",
                rowKey: name,
                vacationDaysLeft: Math.floor(random() * (30 - 5 + 1)) + 5 // Random vacation days between 5 and 30
            };
            return this.tableClient.upsertEntity(employee);
        });
        await Promise.all(tasks);
    }
    async getAllEmployees() {
        console.log("Fetching all employees from the table storage.");
        const result = [];
        for await (const entity of this.tableClient.listEntities()) {
            result.push({
                employeeName: entity.rowKey,
                vacationDaysLeft: entity.vacationDaysLeft
            });
        }
        console.log(`Fetched ${result.length} employees from the table storage.`);
        return result;
    }
    async createTableIfNotExists() {
        try {
            await this.tableClient.createTable();
            console.log("Table created successfully.");
        }
        catch (error) {
            if (error.statusCode === 409) { // 409 indicates the table already exists
                console.log("Table already exists.");
            }
            else {
                throw error;
            }
        }
    }
}
exports.default = EmployeeVacationService;
