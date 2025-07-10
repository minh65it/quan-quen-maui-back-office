using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Linq;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Views.Stores;
using System.Collections.Generic;
using CommunityToolkit.Maui.Views;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores;

public partial class StoreLevelViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Status> _statusNames = new();

    [ObservableProperty]
    private int _selectedStatusIndex = -1;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today;

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today.AddDays(90);

    [ObservableProperty]
    private ObservableCollection<FilterStoreModel> _utilities = new();

    [ObservableProperty]
    private FilterStoreModel _selectedUtility;

    [ObservableProperty]
    private ObservableCollection<FilterStoreModel> selectedUtilities = new();

    [ObservableProperty]
    private ObservableCollection<FilterStoreModel> suitabilities = new();
    [ObservableProperty]
    private ObservableCollection<FilterStoreModel> selectedSuitabilities = new();
    public IRelayCommand<FilterStoreModel> ToggleSuitabilityCommand { get; }

    [ObservableProperty]
    private ObservableCollection<StoreTypeModel> storeTypes = new();

    [ObservableProperty]
    private ObservableCollection<StoreTypeModel> selectedStoreTypes = new();
    public IRelayCommand<StoreTypeModel> ToggleStoreTypeCommand { get; }

    public ICommand ConfirmCommand { get; }

    public IRelayCommand<FilterStoreModel> ToggleUtilityCommand { get; }

    private readonly IStoreUserService _storeUserService;
    private readonly IStoreService _storeService;

    public StoreLevelViewModel()
    {
        _storeUserService = App.Services.GetService<IStoreUserService>();
        _storeService = App.Services.GetService<IStoreService>();
        ConfirmCommand = new AsyncRelayCommand(OnConfirm);
        ToggleUtilityCommand = new RelayCommand<FilterStoreModel>(ToggleUtility);
        ToggleSuitabilityCommand = new RelayCommand<FilterStoreModel>(ToggleSuitability);
        ToggleStoreTypeCommand = new RelayCommand<StoreTypeModel>(ToggleStoreType);
        selectedUtilities = new ObservableCollection<FilterStoreModel>();
        selectedSuitabilities = new ObservableCollection<FilterStoreModel>();
        selectedStoreTypes = new ObservableCollection<StoreTypeModel>();
        InitializeStatusList();
        LoadUtilities();
        LoadSuitabilities();
        LoadStoreTypes();
    }

    partial void OnSelectedUtilitiesChanged(ObservableCollection<FilterStoreModel> value)
    {
        foreach (var u in Utilities)
            u.IsSelected = value.Any(s => s.FilterStoreId == u.FilterStoreId);
    }

    private void ToggleUtility(FilterStoreModel utility)
    {
        if (utility == null) return;
        utility.IsSelected = !utility.IsSelected;
        if (utility.IsSelected)
        {
            if (!SelectedUtilities.Contains(utility))
                SelectedUtilities.Add(utility);
        }
        else
        {
            if (SelectedUtilities.Contains(utility))
                SelectedUtilities.Remove(utility);
        }
        OnPropertyChanged(nameof(SelectedUtilities));
    }

    private void InitializeStatusList()
    {
        StatusNames = new ObservableCollection<Status>(Enum.GetValues(typeof(Status)).Cast<Status>());
    }

    private async void LoadUtilities()
    {
        try
        {
            var list = await _storeService.GetUtilitiesAsync();
            Utilities = new ObservableCollection<FilterStoreModel>(list);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StoreLevelViewModel] LoadUtilities error: {ex.Message}");
        }
    }

    private async void LoadSuitabilities()
    {
        try
        {
            var list = await _storeService.GetSuitabilitiesAsync();
            Suitabilities = new ObservableCollection<FilterStoreModel>(list);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StoreLevelViewModel] LoadSuitabilities error: {ex.Message}");
        }
    }

    private async void LoadStoreTypes()
    {
        try
        {
            var list = await _storeService.GetStoreTypeAsync();
            StoreTypes = new ObservableCollection<StoreTypeModel>(list);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StoreLevelViewModel] LoadStoreTypes error: {ex.Message}");
        }
    }

    private void ToggleSuitability(FilterStoreModel suitability)
    {
        if (suitability == null) return;
        suitability.IsSelected = !suitability.IsSelected;
        if (suitability.IsSelected)
        {
            if (!SelectedSuitabilities.Contains(suitability))
                SelectedSuitabilities.Add(suitability);
        }
        else
        {
            if (SelectedSuitabilities.Contains(suitability))
                SelectedSuitabilities.Remove(suitability);
        }
        OnPropertyChanged(nameof(SelectedSuitabilities));
    }

    private void ToggleStoreType(StoreTypeModel storeType)
    {
        if (storeType == null) return;
        storeType.IsSelected = !storeType.IsSelected;
        if (storeType.IsSelected)
        {
            if (!SelectedStoreTypes.Contains(storeType))
                SelectedStoreTypes.Add(storeType);
        }
        else
        {
            if (SelectedStoreTypes.Contains(storeType))
                SelectedStoreTypes.Remove(storeType);
        }
        OnPropertyChanged(nameof(SelectedStoreTypes));
    }

    private async Task OnConfirm()
    {
        if (_selectedStatusIndex < 0 || _selectedStatusIndex >= _statusNames.Count)
        {
            await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng chọn Trạng thái", "OK");
            return;
        }

        var storeId = StoreRegistrationData.StoreId;
        int levelId = 1;

        // 1. Update Level
        var levelResult = await _storeUserService.UpdateStoreLevelAsync(storeId, levelId, _startDate, _endDate);
        if (!levelResult.IsSuccess)
        {
            System.Diagnostics.Debug.WriteLine($"[StoreLevel] UpdateStoreLevelAsync failed: {levelResult.Message}");
            await App.Current.MainPage.DisplayAlert("Lỗi", levelResult.Message ?? "Có lỗi xảy ra", "OK");
            return;
        }

        // 2. Update Status
        var status = StatusNames[_selectedStatusIndex];
        var storeResult = await _storeService.UpdateStore(storeId, status);
        if (!storeResult.IsSuccess)
        {
            System.Diagnostics.Debug.WriteLine($"[StoreLevel] UpdateStore failed: {storeResult.Message}");
            await App.Current.MainPage.DisplayAlert("Lỗi", $"Cập nhật level thành công nhưng cập nhật trạng thái thất bại: {storeResult.Message}", "OK");
            return;
        }

        // 3. Create Store Type (Utilities and Suitabilities)
        try
        {
            var allSelectedFilterIds = new List<string>();
            
            // Thêm các tiện ích đã chọn
            allSelectedFilterIds.AddRange(SelectedUtilities.Select(u => u.FilterStoreId));
            
            // Thêm các phù hợp đã chọn
            allSelectedFilterIds.AddRange(SelectedSuitabilities.Select(s => s.FilterStoreId));
            
            if (allSelectedFilterIds.Any())
            {
                var storeTypeResult = await _storeService.CreateStoreTypeAsync(allSelectedFilterIds, storeId);
                if (!storeTypeResult.IsSuccess)
                {
                    System.Diagnostics.Debug.WriteLine($"[StoreLevel] CreateStoreTypeAsync failed: {storeTypeResult.Message}");
                    // Không hiển thị lỗi cho user vì đây không phải lỗi nghiêm trọng
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StoreLevel] CreateStoreTypeAsync exception: {ex.Message}");
            // Không hiển thị lỗi cho user vì đây không phải lỗi nghiêm trọng
        }

        // 4. Create Store Type To Store (StoreTypes)
        try
        {
            var selectedStoreTypeIds = SelectedStoreTypes.Select(st => st.storeTypeId).ToList() ;
            
            if (selectedStoreTypeIds.Any())
            {
                var storeTypeToStoreResult = await _storeService.CreateStoreTypeToStoreAsync(selectedStoreTypeIds, storeId);
                if (!storeTypeToStoreResult.IsSuccess)
                {
                    System.Diagnostics.Debug.WriteLine($"[StoreLevel] CreateStoreTypeToStoreAsync failed: {storeTypeToStoreResult.Message}");
                    // Không hiển thị lỗi cho user vì đây không phải lỗi nghiêm trọng
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StoreLevel] CreateStoreTypeToStoreAsync exception: {ex.Message}");
            // Không hiển thị lỗi cho user vì đây không phải lỗi nghiêm trọng
        }

        await App.Current.MainPage.DisplayAlert("Thành công", "Cập nhật level và trạng thái thành công!", "OK");
        await App.Current.MainPage.Navigation.PushAsync(new Views.Stores.StoreMenuPage());
    }
} 