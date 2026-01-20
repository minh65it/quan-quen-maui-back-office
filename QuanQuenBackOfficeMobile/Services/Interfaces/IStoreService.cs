using System.Threading.Tasks;
using QuanQuenBackOfficeMobile.Models;
using System.Collections.Generic;

namespace QuanQuenBackOfficeMobile.Services.Interfaces;

public interface IStoreService
{
    Task<StoreApiResponse> GetAllStoresAsync(int page = 1, int pageSize = 20, string searchText = null);
    Task<CreateStoreResponse> CreateStoreAsync(StoreCreateRequest request, string token = null);
    Task<StoreDetailModel> GetStoreDetailAsync(string storeId);
    Task<ApiResponse> UpdateStore(string storeId, Status status);
    Task<List<FilterStoreModel>> GetUtilitiesAsync();
    Task<List<FilterStoreModel>> GetSuitabilitiesAsync();
    Task<ApiResponse> CreateStoreTypeAsync(List<string> filterStoreIds, string storeId);
    Task<ApiResponse> CreateStoreTypeToStoreAsync(List<string> storeTypeIds, string storeId);
    Task<List<StoreTypeModel>> GetStoreTypeAsync();
    Task<bool> UpdateStoreLocationAsync(string storeId, double latitude, double longitude, string address, string ward = null, string district = null, string city = null);
    Task<ApiResponse> GenerateFoodFromImageAsync(string storeId, List<string> imagePaths);
    Task<ApiResponse> UpdateStoreWithImagesAsync(string storeId, byte[]? frontPicIdentityCard = null, byte[]? backPicIdentityCard = null, byte[]? imageMenu = null);
}