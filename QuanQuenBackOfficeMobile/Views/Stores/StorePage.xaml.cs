namespace QuanQuenBackOfficeMobile.Views;

using QuanQuenBackOfficeMobile.ViewModels;
using QuanQuenBackOfficeMobile.ViewModels.Stores;
using QuanQuenBackOfficeMobile.Views.Stores;
using Microsoft.Maui.Controls;

public partial class StorePage : ContentPage
{
	private StorePageViewModel _viewModel;

	public StorePage(StorePageViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_viewModel?.OnAppearing();
	}

	private async void OnRemainingItemsThresholdReached(object sender, EventArgs e)
	{
		if (_viewModel != null)
		{
			await _viewModel.LoadMoreItemsAsync();
		}
	}
}