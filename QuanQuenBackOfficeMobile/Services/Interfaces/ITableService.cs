using QuanQuenBackOfficeMobile.Models;

namespace QuanQuenBackOfficeMobile.Services.Interfaces
{
    public interface ITableService
    {
        Task<List<TableItem>> GetTablesAsync(string storeId, string tableAreaId, CancellationToken cancellationToken = default);
        Task<ApiResponse> CreateTableAsync(string storeId, string tableAreaId, int tableNumber, string tableName, int tableStatus, int capacity, string? tableDescription, CancellationToken cancellationToken = default);
        Task<ApiResponse> UpdateTableAsync(string tableId, string storeId, string tableAreaId, int tableNumber, string tableName, int tableStatus, int capacity, string? tableDescription, CancellationToken cancellationToken = default);
    }
}
