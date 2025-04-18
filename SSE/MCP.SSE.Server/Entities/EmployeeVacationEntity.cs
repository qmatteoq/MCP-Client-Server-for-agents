using Azure;
using Azure.Data.Tables;

namespace MCP.SSE.Server.Entities
{
    public class EmployeeVacationEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "Employee";
        public string RowKey { get; set; } = string.Empty; // Employee name
        public int VacationDaysLeft { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
