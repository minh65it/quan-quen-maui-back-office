using System.Threading;
using System.Threading.Tasks;
using QuanQuenBackOfficeMobile.Models;

namespace QuanQuenBackOfficeMobile.Services.Interfaces;

public interface IStoreStatisticService
{
    Task<ApiResponseInvoiceListModel> GetInvoicesAsync(string storeName, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ApiResponseMerchantOrderSuccess> GetMerchantOrdersAsync(string storeId, DateTime fromDate, DateTime toDate, string cityId, string storeTypeId = null, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CityItemModel>> GetCitiesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StoreTypeItemModel>> GetStoreTypesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponseOrderList> GetNumberOfUserInOrderAsync(string storeId, DateTime fromDate, DateTime toDate, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ApiResponseLoyalCustomer> GetTotalCustomerCombackAsync(string storeId, int frequency = 2, CancellationToken cancellationToken = default);
    Task<ApiResponseAverageRevenue> GetAverageRevenueAsync(string storeId, CancellationToken cancellationToken = default);
}


