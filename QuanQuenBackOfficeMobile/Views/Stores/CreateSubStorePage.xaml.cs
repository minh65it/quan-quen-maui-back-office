using QuanQuenBackOfficeMobile.ViewModels;

namespace QuanQuenBackOfficeMobile.Views.Stores;

public partial class CreateSubStorePage : ContentPage
{
    private readonly CreateSubStoreViewModel _viewModel;

    public CreateSubStorePage(CreateSubStoreViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        BindingContext = vm;
    }
}
