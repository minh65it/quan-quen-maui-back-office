using QuanQuenBackOfficeMobile.ViewModels.Stores;
using Microsoft.Maui.Controls;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using CommunityToolkit.Maui.Core;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Configuration;

namespace QuanQuenBackOfficeMobile.Views.Stores;

public partial class ContractStorePage : ContentPage
{
    private ContractStoreViewModel _viewModel;

    public ContractStorePage(ContractStoreViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    // Constructor để tạo từ StoreInformationContractViewModel
    public ContractStorePage(StoreInformationContractViewModel sourceViewModel, 
                           ObservableCollection<IDrawingLine> contractLines, 
                           ObservableCollection<IDrawingLine> appendixLines,
                           byte[]? contractSignatureBytes,
                           byte[]? appendixSignatureBytes)
    {
        InitializeComponent();
        
        // Create ViewModel với dependencies
        var contractService = App.Services.GetService<IContractService>();
        var configuration = App.Services.GetService<IConfiguration>();
        
        _viewModel = new ContractStoreViewModel(contractService, configuration);
        
        // Load contract data từ source ViewModel
        _viewModel.LoadContractData(sourceViewModel);
        
        // Set signature data
        _viewModel.SetSignatureData(contractLines, appendixLines, contractSignatureBytes, appendixSignatureBytes);
        
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Auto-load contract when page appears
        try
        {
            System.Diagnostics.Debug.WriteLine("[ContractStorePage] Loading final contract...");
            await _viewModel.SaveContractCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ContractStorePage] Error loading contract: {ex.Message}");
        }
    }

    private async void OnNextClicked(object sender, EventArgs e)
    {
        try
        {
            // Navigate to PhotoStoreUser page without animation để giảm CPU/GPU load
            await Application.Current.MainPage.Navigation.PushAsync(
                new QuanQuenBackOfficeMobile.Views.PhotoStoreUser(), 
                animated: false);
            System.Diagnostics.Debug.WriteLine("[ContractStorePage] Navigated to PhotoStoreUser without animation");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Có lỗi xảy ra khi chuyển trang: {ex.Message}", "OK");
        }
    }
}