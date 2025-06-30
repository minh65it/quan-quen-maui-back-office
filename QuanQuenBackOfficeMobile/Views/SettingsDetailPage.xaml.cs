namespace QuanQuenBackOfficeMobile.Views;

public partial class SettingsDetailPage : ContentPage
{
	public SettingsDetailPage(SettingsDetailViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
