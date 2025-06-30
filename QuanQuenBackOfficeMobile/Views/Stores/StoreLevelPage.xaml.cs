using Microsoft.Maui.Controls;
using QuanQuenBackOfficeMobile.ViewModels.Stores;

namespace QuanQuenBackOfficeMobile.Views.Stores;

public partial class StoreLevelPage : ContentPage
{
    public StoreLevelPage()
    {
        InitializeComponent();
        BindingContext = new StoreLevelViewModel();
    }
} 