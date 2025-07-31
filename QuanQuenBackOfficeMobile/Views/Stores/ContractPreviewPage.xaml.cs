namespace QuanQuenBackOfficeMobile.Views.Stores;

using QuanQuenBackOfficeMobile.ViewModels.Stores;

public partial class ContractPreviewPage : ContentPage
{
    public ContractPreviewPage(ContractPreviewViewModel viewModel)
    {
        InitializeComponent();
        this.BindingContext = viewModel;
    }
} 