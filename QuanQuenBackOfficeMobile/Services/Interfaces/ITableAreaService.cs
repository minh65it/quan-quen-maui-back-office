using QuanQuenBackOfficeMobile.Models;

namespace QuanQuenBackOfficeMobile.Services.Interfaces
{
    public interface ITableAreaService
    {
        Task<List<TableAreaItem>> GetTableAreasAsync(string storeId, CancellationToken cancellationToken = default);
        Task<ApiResponse> CreateTableAreaAsync(string storeId, string areaName, string? areaDescription, int order, CancellationToken cancellationToken = default);
        Task<ApiResponse> UpdateTableAreaAsync(string storeId, string tableAreaId, string areaName, string? areaDescription, int order, CancellationToken cancellationToken = default);
    }
}


