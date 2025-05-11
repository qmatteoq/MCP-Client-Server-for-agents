import { TableClient, TableEntityResult } from "@azure/data-tables";

interface EmployeeVacationEntity {
    partitionKey: string;
    rowKey: string;
    vacationDaysLeft: number;
}

interface Employee {
    employeeName: string;
    vacationDaysLeft: number;
}

class EmployeeVacationService {
    private tableClient: TableClient;

    constructor(connectionString: string, tableName: string) {
        this.tableClient = TableClient.fromConnectionString(connectionString, tableName);
    }

    async getVacationDaysLeft(employeeName: string): Promise<number | null> {
        try {
            const entity = await this.tableClient.getEntity<EmployeeVacationEntity>("Employee", employeeName);
            console.log(`Fetched vacation days left for ${employeeName}: ${entity.vacationDaysLeft}`);
            return entity.vacationDaysLeft;
        } catch (error: any) {
            if (error.statusCode === 404) {
                return null;
            }
            throw error;
        }
    }

    async chargeVacationDays(employeeName: string, daysToCharge: number): Promise<boolean> {
        try {
            const entityResult = await this.tableClient.getEntity<EmployeeVacationEntity>("Employee", employeeName);
            const entity = entityResult as EmployeeVacationEntity;

            if (entity.vacationDaysLeft < daysToCharge) {
                return false;
            }

            entity.vacationDaysLeft -= daysToCharge;
            await this.tableClient.updateEntity(entity, "Merge");
            console.log(`Charged ${daysToCharge} vacation days to ${employeeName}. Remaining days: ${entity.vacationDaysLeft}`);
            return true;
        } catch (error: any) {
            if (error.statusCode === 404) {
                return false;
            }
            throw error;
        }
    }

    async isTableEmpty(): Promise<boolean> {
        const entities = this.tableClient.listEntities<EmployeeVacationEntity>();
        for await (const _ of entities) {
            console.log("Table is not empty.");
            return false;
        }
        console.log("Table is empty.");
        return true;
    }

    async seedFakeEmployees(): Promise<void> {
        const random = Math.random;
        const names = [
            "Alice Johnson", "Bob Smith", "Charlie Lee", "Diana Evans", "Ethan Brown",
            "Fiona Clark", "George Miller", "Hannah Davis", "Ian Wilson", "Julia Adams"
        ];

        const tasks = names.map(name => {
            const employee: EmployeeVacationEntity = {
                partitionKey: "Employee",
                rowKey: name,
                vacationDaysLeft: Math.floor(random() * (30 - 5 + 1)) + 5 // Random vacation days between 5 and 30
            };
            return this.tableClient.upsertEntity(employee);
        });

        await Promise.all(tasks);
    }

    async getAllEmployees(): Promise<Employee[]> {
        console.log("Fetching all employees from the table storage.");
        const result: Employee[] = [];

        for await (const entity of this.tableClient.listEntities<EmployeeVacationEntity>()) {
            result.push({
                employeeName: entity.rowKey,
                vacationDaysLeft: entity.vacationDaysLeft
            });
        }

        console.log(`Fetched ${result.length} employees from the table storage.`);
        return result;
    }

    async createTableIfNotExists(): Promise<void> {
        try {
            await this.tableClient.createTable();
            console.log("Table created successfully.");
        } catch (error: any) {
            if (error.statusCode === 409) { // 409 indicates the table already exists
                console.log("Table already exists.");
            } else {
                throw error;
            }
        }
    }
}

export default EmployeeVacationService;