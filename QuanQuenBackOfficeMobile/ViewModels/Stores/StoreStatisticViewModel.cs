using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Services;
using System;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores;

public partial class StoreStatisticViewModel : ObservableObject
{
    private readonly IStoreStatisticService _storeStatisticService;
    private readonly GeographicApiService _geographicApiService;
    private readonly IStoreService _storeService;

    [ObservableProperty]
    private string storeName;

    [ObservableProperty]
    private bool isLoading;

    public ObservableCollection<InvoiceItemModel> Invoices { get; } = new();

    // Filters and orders data
    [ObservableProperty]
    private string storeId;

    [ObservableProperty]
    private string selectedCityId;

    [ObservableProperty]
    private string selectedStoreTypeId;

    [ObservableProperty]
    private DateTime fromDate;

    [ObservableProperty]
    private DateTime toDate;

    public ObservableCollection<CityItemModel> Cities { get; } = new();
    public ObservableCollection<StoreTypeItemModel> StoreTypes { get; } = new();
    public ObservableCollection<OrderItemModel> Orders { get; } = new();
    public ObservableCollection<OrderItemModel> UsersInOrders { get; } = new();
    public ObservableCollection<LoyalCustomerItemModel> LoyalCustomers { get; } = new();

    [ObservableProperty]
    private int frequency = 2;

    [ObservableProperty]
    private AverageRevenueDataModel averageRevenue;

    [ObservableProperty]
    private CityItemModel selectedCity;

    [ObservableProperty]
    private StoreTypeItemModel selectedStoreType;

    partial void OnSelectedCityChanged(CityItemModel value)
    {
        SelectedCityId = value?.id;
    }

    partial void OnSelectedStoreTypeChanged(StoreTypeItemModel value)
    {
        SelectedStoreTypeId = value?.id;
    }

    public StoreStatisticViewModel(IStoreStatisticService storeStatisticService, GeographicApiService geographicApiService, IStoreService storeService, string storeName, string storeId)
    {
        _storeStatisticService = storeStatisticService;
        _geographicApiService = geographicApiService;
        _storeService = storeService;
        StoreName = storeName;
        StoreId = storeId;
        // Defaults for orders filter
        FromDate = new DateTime(2025, 1, 1);
        ToDate = DateTime.UtcNow;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsLoading || string.IsNullOrWhiteSpace(StoreName)) return;
        try
        {
            IsLoading = true;
            Invoices.Clear();
            var res = await _storeStatisticService.GetInvoicesAsync(StoreName);
            var items = res?.data?.result;
            if (items != null)
            {
                foreach (var item in items)
                {
                    Invoices.Add(item);
                }
            }
            // Load cities (zones) via GeographicApiService
            Cities.Clear();
            var provinces = await _geographicApiService.GetProvincesAsync();
            if (provinces != null)
            {
                foreach (var p in provinces)
                {
                    Cities.Add(new CityItemModel { id = p.zoneId, name = p.zoneName });
                }
                // default select HCM if present
                SelectedCity = Cities.FirstOrDefault(x => x.name.Contains("Hồ Chí Minh", StringComparison.OrdinalIgnoreCase))
                                ?? Cities.FirstOrDefault();
            }

            // Load store types via StoreService
            StoreTypes.Clear();
            var st = await _storeService.GetStoreTypeAsync();
            if (st != null)
            {
                foreach (var s in st)
                {
                    StoreTypes.Add(new StoreTypeItemModel { id = s.storeTypeId, name = s.storeTypeName });
                }
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadOrdersAsync()
    {
        if (IsLoading || string.IsNullOrWhiteSpace(StoreId) || string.IsNullOrWhiteSpace(SelectedCityId)) return;
        try
        {
            IsLoading = true;
            Orders.Clear();
            var res = await _storeStatisticService.GetMerchantOrdersAsync(StoreId, FromDate, ToDate, SelectedCityId, SelectedStoreTypeId);
            var list = res?.data?.listOrder?.result;
            if (list != null)
            {
                // sort: mặc định có thể theo createdDate desc
                foreach (var o in list.OrderByDescending(x => x.createdDate))
                {
                    Orders.Add(o);
                }
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadUsersInOrdersAsync()
    {
        if (IsLoading || string.IsNullOrWhiteSpace(StoreId)) return;
        try
        {
            IsLoading = true;
            UsersInOrders.Clear();
            var res = await _storeStatisticService.GetNumberOfUserInOrderAsync(StoreId, FromDate, ToDate);
            var list = res?.data?.result;
            if (list != null)
            {
                foreach (var o in list.OrderByDescending(x => x.createdDate))
                {
                    UsersInOrders.Add(o);
                }
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadLoyalCustomersAsync()
    {
        if (IsLoading || string.IsNullOrWhiteSpace(StoreId)) return;
        try
        {
            IsLoading = true;
            LoyalCustomers.Clear();
            var res = await _storeStatisticService.GetTotalCustomerCombackAsync(StoreId, Frequency);
            var list = res?.data?.listCustomerComback;
            if (list != null)
            {
                foreach (var c in list)
                {
                    LoyalCustomers.Add(c);
                }
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadAverageRevenueAsync()
    {
        if (IsLoading || string.IsNullOrWhiteSpace(StoreId)) return;
        try
        {
            IsLoading = true;
            var res = await _storeStatisticService.GetAverageRevenueAsync(StoreId);
            AverageRevenue = res?.data;
        }
        finally
        {
            IsLoading = false;
        }
    }
}


