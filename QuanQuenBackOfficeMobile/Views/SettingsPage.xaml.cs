namespace QuanQuenBackOfficeMobile.Views;

public partial class SettingsPage : ContentPage
{
	SettingsViewModel ViewModel;

	public SettingsPage(SettingsViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = ViewModel = viewModel;
	}

	protected override async void OnNavigatedTo(NavigatedToEventArgs args)
	{
		base.OnNavigatedTo(args);

		await ViewModel.LoadDataAsync();
	}
}
