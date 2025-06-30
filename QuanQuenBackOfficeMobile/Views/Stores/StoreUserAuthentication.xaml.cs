using QuanQuenBackOfficeMobile.ViewModels.Stores;
using Microsoft.Maui.Controls;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.Views.Stores;

public partial class StoreUserAuthentication : ContentPage
{
    public StoreUserAuthentication(string email)
    {
        InitializeComponent();
        var service = App.Services.GetService<IStoreUserService>();
        BindingContext = new StoreUserAuthenticationViewModel(service, email);
    }
} 