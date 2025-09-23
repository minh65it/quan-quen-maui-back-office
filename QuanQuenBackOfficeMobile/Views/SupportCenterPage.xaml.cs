using QuanQuenBackOfficeMobile.ViewModels;

namespace QuanQuenBackOfficeMobile.Views;

public partial class SupportCenterPage : ContentPage
{
	public SupportCenterPage(SupportCenterViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}


