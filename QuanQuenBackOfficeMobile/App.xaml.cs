using QuanQuenBackOfficeMobile.Services;
using QuanQuenBackOfficeMobile.Views;
using QuanQuenBackOfficeMobile.ViewModels;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.ApplicationModel;
#if ANDROID
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
#endif

namespace QuanQuenBackOfficeMobile;

public partial class App : Application
{
	public static IServiceProvider Services { get; private set; }


    public App()
	{
		InitializeComponent();
		Application.Current.UserAppTheme = AppTheme.Light;
        MainPage = new ContentPage { Content = new ActivityIndicator { IsRunning = true, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center } };
		Dispatcher.Dispatch(async () => await CheckTokenAndNavigateAsync());
	}

	private async Task CheckTokenAndNavigateAsync()
	{
		try
		{
			await Task.Yield();
        var sp = IPlatformApplication.Current.Services;
			Services = sp;
			var tokenService = sp.GetService<ITokenService>();
			if (tokenService != null && await tokenService.IsTokenValidAsync().ConfigureAwait(false))
			{
				var mainViewModel = sp.GetService<MainViewModel>();
				MainThread.BeginInvokeOnMainThread(() => MainPage = new MainPage(mainViewModel));
			}
			else
			{
				var loginViewModel = sp.GetService<LoginViewModel>();
				MainThread.BeginInvokeOnMainThread(() => MainPage = new LoginPage(loginViewModel));
			}
		}
		catch
		{
			var loginViewModel = IPlatformApplication.Current.Services.GetService<LoginViewModel>();
			MainThread.BeginInvokeOnMainThread(() => MainPage = new LoginPage(loginViewModel));
		}
	}
}
