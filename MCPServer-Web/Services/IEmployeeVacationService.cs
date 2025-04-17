using MCPServer_Web.Entities;

namespace MCPServer_Web.Services
{
    public interface IEmployeeVacationService
    {
        Task<int?> GetVacationDaysLeftAsync(string employeeName);
        Task<bool> ChargeVacationDaysAsync(string employeeName, int daysToCharge);
        Task<List<Employee>> GetAllEmployeesAsync();
    }
}
