using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuanQuenBackOfficeMobile.Models;

namespace QuanQuenBackOfficeMobile.Services.Interfaces
{
    public interface IFoodFilterService
    {
        Task<List<FoodFilterItem>> GetFoodFiltersAsync(string storeId, CancellationToken cancellationToken = default);
        Task<bool> UpdateFoodFilterAsync(string filterFoodId, string storeId, string name, string value, CancellationToken cancellationToken = default);
        Task<bool> CreateFoodFilterAsync(string storeId, string name, string value, CancellationToken cancellationToken = default);
        Task<string> CreateFoodAsync(string storeId, string filterFoodId, string name, string description, decimal price, decimal priceSale, string foodUnitId, bool isCombo = false, bool isOutOfStock = false, CancellationToken cancellationToken = default);
    }
}

