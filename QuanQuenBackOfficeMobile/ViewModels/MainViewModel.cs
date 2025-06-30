using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using Microsoft.Maui.Storage;

namespace QuanQuenBackOfficeMobile.ViewModels;

public partial class MainViewModel : BaseViewModel
{
	private readonly IAuthService _authService;

	public MainViewModel(IAuthService authService)
	{
		_authService = authService;
	}

	public void DoSomething1()
	{
		for (int i = 0; i < 10; i++)
		{
			System.Diagnostics.Debug.WriteLine(i);
		}
	}

	public void DoSomething2()
	{
		foreach (var i in Enumerable.Range(1, 10))
		{
			System.Diagnostics.Debug.WriteLine(i);
		}
	}

	[RelayCommand]
	private async Task Logout()
	{
		try
		{
			var result = await _authService.LogoutAsync();
			var loginViewModel = IPlatformApplication.Current.Services.GetService<LoginViewModel>();
			Application.Current.MainPage = new LoginPage(loginViewModel);
		}
		catch (Exception ex)
		{
			await Shell.Current.DisplayAlert("Lỗi", $"Không thể đăng xuất. Vui lòng thử lại. {ex.Message}", "OK");
		}
	}
}

