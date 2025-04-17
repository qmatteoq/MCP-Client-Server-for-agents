using Azure;
using Azure.Data.Tables;
using MCPServer_Web.Entities;

namespace MCPServer_Web.Services
{

    public class EmployeeVacationService : IEmployeeVacationService
    {
        private readonly TableClient _tableClient;

        public EmployeeVacationService(string storageConnectionString, string tableName)
        {
            _tableClient = new TableClient(storageConnectionString, tableName);
            _tableClient.CreateIfNotExists();
        }

        public async Task<int?> GetVacationDaysLeftAsync(string employeeName)
        {
            try
            {
                var entity = await _tableClient.GetEntityAsync<EmployeeVacationEntity>("Employee", employeeName);
                return entity.Value.VacationDaysLeft;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<bool> ChargeVacationDaysAsync(string employeeName, int daysToCharge)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<EmployeeVacationEntity>("Employee", employeeName);
                var entity = response.Value;
                if (entity.VacationDaysLeft < daysToCharge)
                    return false;
                entity.VacationDaysLeft -= daysToCharge;
                await _tableClient.UpdateEntityAsync(entity, entity.ETag);
                return true;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return false;
            }
        }

        public async Task<bool> IsTableEmptyAsync()
        {
            await foreach (var _ in _tableClient.QueryAsync<EmployeeVacationEntity>(maxPerPage: 1))
            {
                return false;
            }
            return true;
        }

        public async Task SeedFakeEmployeesAsync()
        {
            var random = new Random();
            var names = new List<string>
            {
                "Alice Johnson", "Bob Smith", "Charlie Lee", "Diana Evans", "Ethan Brown",
                "Fiona Clark", "George Miller", "Hannah Davis", "Ian Wilson", "Julia Adams"
            };
            var tasks = new List<Task>();
            foreach (var name in names)
            {
                var employee = new EmployeeVacationEntity
                {
                    RowKey = name,
                    VacationDaysLeft = random.Next(5, 31) // Random vacation days between 5 and 30
                };
                tasks.Add(_tableClient.UpsertEntityAsync(employee));
            }
            await Task.WhenAll(tasks);
        }

        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            var result = new List<Employee>();
            await foreach (var entity in _tableClient.QueryAsync<EmployeeVacationEntity>())
            {
                result.Add(new Employee
                {
                    EmployeeName = entity.RowKey,
                    VacationDaysLeft = entity.VacationDaysLeft
                });
            }
            return result;
        }
    }
}
