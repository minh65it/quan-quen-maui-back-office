using QuanQuenBackOfficeMobile.ViewModels;

namespace QuanQuenBackOfficeMobile.Views;

public partial class AiChatbotPage : ContentPage
{
    public AiChatbotPage(AiChatbotViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}


