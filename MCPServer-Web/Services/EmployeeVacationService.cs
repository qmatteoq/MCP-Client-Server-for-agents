using Azure;
using Azure.Data.Tables;
using MCPServer_Web.Entities;

namespace MCPServer_Web.Services
{

    public class EmployeeVacationService : IEmployeeVacationService
    {
        private readonly TableClient _tableClient;

        private readonly ILogger<EmployeeVacationService> _logger;

        public EmployeeVacationService(string storageConnectionString, string tableName, ILogger<EmployeeVacationService> logger)
        {
            _tableClient = new TableClient(storageConnectionString, tableName);
            _tableClient.CreateIfNotExists();
             _logger = logger;
        }

        public async Task<int?> GetVacationDaysLeftAsync(string employeeName)
        {
            try
            {
                var entity = await _tableClient.GetEntityAsync<EmployeeVacationEntity>("Employee", employeeName);
                _logger.LogInformation($"Fetched vacation days left for {employeeName}: {entity.Value.VacationDaysLeft}");
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
                _logger.LogInformation($"Charged {daysToCharge} vacation days to {employeeName}. Remaining days: {entity.VacationDaysLeft}");
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
                _logger.LogInformation("Table is not empty.");
                return false;
            }
            _logger.LogInformation("Table is empty.");
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
            _logger.LogInformation("Fetching all employees from the table storage.");
            var result = new List<Employee>();
            await foreach (var entity in _tableClient.QueryAsync<EmployeeVacationEntity>())
            {
                result.Add(new Employee
                {
                    EmployeeName = entity.RowKey,
                    VacationDaysLeft = entity.VacationDaysLeft
                });
            }

            _logger.LogInformation($"Fetched {result.Count} employees from the table storage.");
            return result;
        }
    }
}
