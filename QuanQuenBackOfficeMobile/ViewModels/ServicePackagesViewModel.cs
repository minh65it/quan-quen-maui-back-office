using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.ViewModels;

public partial class ServicePackagesViewModel : ObservableObject
{
    private readonly IServicePackagesService _service;

    public ObservableCollection<string> QuanMoiItems { get; } = new();
    public ObservableCollection<string> QuanQuenItems { get; } = new();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? error;

    [ObservableProperty]
    private string selectedPackageKey = "";

    public ServicePackagesViewModel(IServicePackagesService service)
    {
        _service = service;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        Error = null;
        try
        {
            var dto = await _service.GetAsync();
            FillList(QuanMoiItems, dto.QuanMoi);
            FillList(QuanQuenItems, dto.QuanQuen);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static void FillList(ObservableCollection<string> target, IEnumerable<string> items)
    {
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }
    }
}


