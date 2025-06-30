using CommunityToolkit.Mvvm.ComponentModel;

namespace QuanQuenBackOfficeMobile.ViewModels;

public partial class CsrDetailPopupViewModel : ObservableObject
{
    [ObservableProperty]
    private string htmlContent;
    [ObservableProperty]
    private string title;
} 