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
		
		Dispatcher.Dispatch(async () => CheckTokenAndNavigateAsync());
    }

	private async Task CheckTokenAndNavigateAsync()
	{
		var sp = IPlatformApplication.Current.Services;
		Services = sp;
        var tokenService = sp.GetService<ITokenService>();
        if (tokenService != null && await tokenService.IsTokenValidAsync())
		{
			var mainViewmodel = sp.GetService<MainViewModel>();
			MainPage = new MainPage(mainViewmodel!);
        }
		else
		{
			// Set AppShell as MainPage to enable Shell navigation
			MainPage = new AppShell();
        }
           
	}
}
