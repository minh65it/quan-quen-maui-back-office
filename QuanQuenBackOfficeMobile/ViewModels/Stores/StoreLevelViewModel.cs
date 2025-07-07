using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Linq;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Models;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores;

public partial class StoreLevelViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Status> _statusNames = new();

    [ObservableProperty]
    private ObservableCollection<string> _levelNames = new();

    [ObservableProperty]
    private int _selectedLevelIndex = -1;

    [ObservableProperty]
    private int _selectedStatusIndex = -1;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today;

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today.AddDays(90);

    public ICommand ConfirmCommand { get; }

    private List<LevelDto> _levels = new();
    private readonly IStoreUserService _storeUserService;
    private readonly IStoreService _storeService;

    public StoreLevelViewModel()
    {
        _storeUserService = App.Services.GetService<IStoreUserService>();
        _storeService = App.Services.GetService<IStoreService>();
        ConfirmCommand = new AsyncRelayCommand(OnConfirm);
        _ = LoadLevelsAsync();
        InitializeStatusList();
    }

    private void InitializeStatusList()
    {
        StatusNames = new ObservableCollection<Status>(Enum.GetValues(typeof(Status)).Cast<Status>());
    }

    private async Task LoadLevelsAsync()
    {
        try
        {
            var response = await _storeUserService.GetListLevelAsync();
            if (response?.Result != null)
            {
                _levels = response.Result;
                LevelNames = new ObservableCollection<string>(_levels.Select(l => l.LevelName));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StoreLevel] LoadLevelsAsync error: {ex.Message}");
        }
    }

    private async Task OnConfirm()
    {
        if (_selectedLevelIndex < 0 || _selectedLevelIndex >= _levels.Count)
        {
            await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng chọn Level", "OK");
            return;
        }

        if (_selectedStatusIndex < 0 || _selectedStatusIndex >= _statusNames.Count)
        {
            await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng chọn Trạng thái", "OK");
            return;
        }

        var selectedLevel = _levels[_selectedLevelIndex];
        var storeId = StoreRegistrationData.StoreId;

        // 1. Update Level
        var levelResult = await _storeUserService.UpdateStoreLevelAsync(storeId, selectedLevel.Id, _startDate, _endDate);
        if (!levelResult.IsSuccess)
        {
            System.Diagnostics.Debug.WriteLine($"[StoreLevel] UpdateStoreLevelAsync failed: {levelResult.Message}");
            await App.Current.MainPage.DisplayAlert("Lỗi", levelResult.Message ?? "Cập nhật level thất bại", "OK");
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

        await App.Current.MainPage.DisplayAlert("Thành công", "Cập nhật level và trạng thái thành công!", "OK");
        await App.Current.MainPage.Navigation.PopToRootAsync();
    }
} 