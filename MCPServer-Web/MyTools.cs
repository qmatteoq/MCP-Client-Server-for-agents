using System.ComponentModel;
using MCPServer_Web.Entities;
using MCPServer_Web.Services;
using ModelContextProtocol.Server;

[McpServerToolType]
public class EmployeeVacationTool
{
    private readonly IEmployeeVacationService _employeeVacationService;
    public EmployeeVacationTool(IEmployeeVacationService employeeVacationService)
    {
        _employeeVacationService = employeeVacationService;
    }

    [McpServerTool, Description("Get the vacation days left for a given employee.")]
    public async Task<int?> GetVacationDaysLeftAsync([Description("The name of the employee")] string employeeName)
    {
        return await _employeeVacationService.GetVacationDaysLeftAsync(employeeName);
    }

    [McpServerTool, Description("Charge vacation days for a given employee.")]
    public async Task<bool> ChargeVacationDaysAsync([Description("The name of the employee")] string employeeName, [Description("The number of days to charge")] int daysToCharge)
    {
        return await _employeeVacationService.ChargeVacationDaysAsync(employeeName, daysToCharge);
    }

    [McpServerTool, Description("Get the list of employees with their number of vacation days left")]
    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        return await _employeeVacationService.GetAllEmployeesAsync();
    }
}
