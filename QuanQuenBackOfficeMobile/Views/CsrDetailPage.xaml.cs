using QuanQuenBackOfficeMobile.ViewModels;

namespace QuanQuenBackOfficeMobile.Views;

public partial class CsrDetailPage : ContentPage
{
    public CsrDetailPage(string title, string html)
    {
        InitializeComponent();
        BindingContext = new CsrDetailPopupViewModel { Title = title, HtmlContent = html };
    }
} 