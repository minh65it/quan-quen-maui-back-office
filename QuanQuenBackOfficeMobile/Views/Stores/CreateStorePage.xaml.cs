using QuanQuenBackOfficeMobile.ViewModels;
using QuanQuenBackOfficeMobile.Views.Stores;

namespace QuanQuenBackOfficeMobile.Views;

public partial class CreateStorePage : ContentPage
{
	private readonly CreateStoreViewModel _viewModel;

	public CreateStorePage(CreateStoreViewModel vm)
	{
		InitializeComponent();
		_viewModel = vm;
		BindingContext = vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (BindingContext is CreateStoreViewModel vm)
			await vm.InitAsync();
	}

	private async void OnNextButtonClicked(object sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(_viewModel.Email))
		{
			await DisplayAlert("Lỗi", "Vui lòng nhập email", "OK");
			return;
		}

		var page = new CreateStoreUserPasswordPage(_viewModel.Email);
		await Navigation.PushAsync(page);
	}
}