using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.ViewModels.Stores;
using QuanQuenBackOfficeMobile.Views.Stores;
using System.Reflection;
#if IOS && ANDROID
using QuanQuenBackOfficeMobile.Controls;
#endif

namespace QuanQuenBackOfficeMobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseSkiaSharp()
#if ANDROID || IOS
			.UseMauiMaps()
#endif
			.UseMauiCommunityToolkitMediaElement()
			.UseMauiCommunityToolkit()
			.UseMauiCommunityToolkitCamera()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialSymbol");
				fonts.AddFont("FontAwesome6FreeBrands.otf", "FontAwesomeBrands");
				fonts.AddFont("FontAwesome6FreeRegular.otf", "FontAwesomeRegular");
				fonts.AddFont("FontAwesome6FreeSolid.otf", "FontAwesomeSolid");
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "QuanQuenBackOfficeMobile.appsettings.json";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{resourceName}' not found. " +
                "Ensure the file is in the project root, Build Action=EmbeddedResource, and the namespace matches.");

        builder.Configuration.AddJsonStream(stream);


        builder.Services.AddHttpClient<IAuthService, AuthService>((sp, client) =>
               {
                   var cfg = sp.GetRequiredService<IConfiguration>();
                   var baseUrl = cfg["API:Identity:BaseUrl"]
                                 ?? throw new InvalidOperationException(
                                     "Configuration key 'API:Identity:BaseUrl' is missing.");
                   client.BaseAddress = new Uri(baseUrl);
                   client.Timeout = TimeSpan.FromSeconds(30);
               });

        builder.Services.AddHttpClient<IStoreService, StoreService>((sp, client) =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var baseUrl = cfg["API:Store:BaseUrl"]
                          ?? throw new InvalidOperationException("Missing API:Store:BaseUrl");
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        builder.Services.AddHttpClient<IStoreUserService, StoreUserService>((sp, client) =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var baseUrl = cfg["API:Identity:BaseUrl"]
                          ?? throw new InvalidOperationException("Missing API:Identity:BaseUrl");
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        builder.Services.AddHttpClient<IStoreMediaService, StoreMediaService>((sp, client) =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var baseUrl = cfg["API:Store:BaseUrl"]
                          ?? throw new InvalidOperationException("Missing API:Store:BaseUrl");
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

      

        // Register ViewModels
        builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<MainViewModel>();
		builder.Services.AddTransient<CreateStoreUserPasswordViewModel>();
        // Register Views
        builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<RegisterPage>();
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<CreateStorePage>();

		builder.Services.AddSingleton<SampleViewModel>();

		builder.Services.AddSingleton<MediaElementViewModel>();
		builder.Services.AddSingleton<MediaElementPage>();

		builder.Services.AddSingleton<WebViewViewModel>();
		builder.Services.AddSingleton<WebViewPage>();

		builder.Services.AddSingleton<AboutViewModel>();
		builder.Services.AddSingleton<AboutPage>();

		builder.Services.AddSingleton<LocalizationViewModel>();
		builder.Services.AddSingleton<LocalizationPage>();

		builder.Services.AddSingleton<MapViewModel>();
		builder.Services.AddSingleton<MapPage>();

		builder.Services.AddTransient<SampleDataService>();
		builder.Services.AddTransient<SettingsDetailViewModel>();
		builder.Services.AddTransient<SettingsDetailPage>();

		builder.Services.AddSingleton<SettingsViewModel>();
		builder.Services.AddSingleton<SettingsPage>();

		builder.Services.AddSingleton<DrawingExampleViewModel>();
		builder.Services.AddSingleton<DrawingExamplePage>();

		builder.Services.AddSingleton<ExampleFontAwesomeUsageViewModel>();
		builder.Services.AddSingleton<ExampleFontAwesomeUsagePage>();

		builder.Services.AddSingleton<ExampleMaterialFontUsageViewModel>();
		builder.Services.AddSingleton<ExampleMaterialFontUsagePage>();

		builder.Services.AddSingleton<LottieViewModel>();
		builder.Services.AddSingleton<LottiePage>();

		builder.Services.AddTransient<StorePageViewModel>();
		builder.Services.AddTransient<StorePage>();
		builder.Services.AddTransient<StoreDetailViewModel>();
		builder.Services.AddTransient<StoreDetailPage>();

		builder.Services.AddSingleton<ITokenService, TokenService>();

		builder.Services.AddSingleton<GeographicApiService>();

		builder.Services.AddTransient<CreateStoreViewModel>();

		builder.Services.AddTransient<PhotoStoreUserViewModel>();
		builder.Services.AddTransient<PhotoStoreUser>();

		builder.Services.UsePageResolver();

#if DEBUG
		builder.Logging.AddDebug();
#endif
		#if IOS && ANDROID
    builder.ConfigureMauiHandlers(handlers =>
    {
        handlers.AddHandler(typeof(CustomDropdownPicker), typeof(CustomDropdownPickerHandler));
    });
#endif
		return builder.Build();
	}
}
