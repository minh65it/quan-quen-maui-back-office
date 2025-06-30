using QuanQuenBackOfficeMobile.ViewModels;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.Views.Stores;

public partial class CreateStoreUserPasswordPage : ContentPage
{
	public CreateStoreUserPasswordPage(string email)
	{
		InitializeComponent();
		var userService = App.Services.GetService<IStoreUserService>();
		var storeService = App.Services.GetService<IStoreService>();
		var storeMediaService = App.Services.GetService<IStoreMediaService>();
        var viewModel = new CreateStoreUserPasswordViewModel(userService!, storeService! ,storeMediaService! );
		viewModel.Email = email;
		BindingContext = viewModel;
	}
}