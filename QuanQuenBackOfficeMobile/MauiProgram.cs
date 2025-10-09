using Camera.MAUI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuanQuenBackOfficeMobile.Services;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.ViewModels;
using QuanQuenBackOfficeMobile.ViewModels.Stores;
using QuanQuenBackOfficeMobile.Views.Stores;
using System.Reflection;
using ZXing.Net.Maui.Controls;
#if IOS && ANDROID
using QuanQuenBackOfficeMobile.Controls;
using Camera.MAUI;
#endif

namespace QuanQuenBackOfficeMobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
#if ANDROID || IOS
			.UseMauiMaps()
			
#endif
			.UseBarcodeReader()
			.UseMauiCameraView()
            .UseMauiCommunityToolkitMediaElement()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialSymbol");
				fonts.AddFont("FontAwesome6FreeBrands.otf", "FontAwesomeBrands");
				fonts.AddFont("FontAwesome6FreeRegular.otf", "FontAwesomeRegular");
				fonts.AddFont("FontAwesome6FreeSolid.otf", "FontAwesomeSolid");
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        // Load configuration based on environment
        var assembly = Assembly.GetExecutingAssembly();
        
#if DEBUG
        // Development environment - load appsettings.Development.json
        var resourceName = "QuanQuenBackOfficeMobile.appsettings.Development.json";
#else
        var resourceName = "QuanQuenBackOfficeMobile.appsettings.json";
#endif

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{resourceName}' not found. " +
                "Ensure the file is in the project root, Build Action=EmbeddedResource, and the namespace matches.");

        builder.Configuration.AddJsonStream(stream);

        // Register ConfigurationService
        builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();

        builder.Services.AddHttpClient<IAuthService, AuthService>((sp, client) =>
               {
                   var configService = sp.GetRequiredService<IConfigurationService>();
                   var baseUrl = configService.GetApiUrl("Identity");
                   client.BaseAddress = new Uri(baseUrl);
				   client.Timeout = TimeSpan.FromMinutes(5);

               });

        builder.Services.AddHttpClient<IStoreService, StoreService>((sp, client) =>
        {
            var configService = sp.GetRequiredService<IConfigurationService>();
            var baseUrl = configService.GetApiUrl("Store");
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromMinutes(5); 

        });

        builder.Services.AddHttpClient<IStoreUserService, StoreUserService>((sp, client) =>
        {
            var configService = sp.GetRequiredService<IConfigurationService>();
            var baseUrl = configService.GetApiUrl("Identity");
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromMinutes(5); 

        });

        builder.Services.AddHttpClient<IStoreMediaService, StoreMediaService>((sp, client) =>
        {
            var configService = sp.GetRequiredService<IConfigurationService>();
            var baseUrl = configService.GetApiUrl("Store");
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromMinutes(5); 
        });

        builder.Services.AddHttpClient<IQRCodeService, QRCodeService>((sp, client) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var baseUrl = configuration["QRCode:BaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                client.BaseAddress = new Uri(baseUrl);
            }
            client.Timeout = TimeSpan.FromMinutes(5); 
        });

        // Register QR Code Scanning Service

        builder.Services.AddHttpClient<IAiChatbotService, AiChatbotService>((sp, client) =>
        {
            var configService = sp.GetRequiredService<IConfigurationService>();
            var baseUrl = configService.GetApiUrl("AiChatbot");
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        builder.Services.AddHttpClient<IAffiliateService, AffiliateService>((sp, client) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var baseUrl = configuration["API:Affiliate:BaseUrl"];
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        builder.Services.AddHttpClient<IStoreStatisticService, StoreStatisticService>((sp, client) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var baseUrl = configuration["API:Payment:BaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                client.BaseAddress = new Uri(baseUrl);
            }
            client.Timeout = TimeSpan.FromMinutes(2);
        });

      

        // Register ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<ServicePackagesViewModel>();
        builder.Services.AddTransient<AffiliateLoginViewModel>();
        builder.Services.AddTransient<AffiliateRegisterViewModel>();
        builder.Services.AddTransient<AffiliateCreateStoreViewModel>();
        builder.Services.AddTransient<AffiliateStoreUserPasswordViewModel>();
        builder.Services.AddTransient<AffiliateStoreUserAuthenticationViewModel>();
        builder.Services.AddTransient<AffiliateStoreListViewModel>();
        builder.Services.AddTransient<QuanQuenBackOfficeMobile.ViewModels.Affiliates.AffiliateAgreementInformationViewModel>();
        builder.Services.AddTransient<QuanQuenBackOfficeMobile.ViewModels.Affiliates.AffiliateAgreementPreviewViewModel>();
        builder.Services.AddTransient<QuanQuenBackOfficeMobile.ViewModels.Affiliates.AffiliateAgreementSignatureViewModel>();
        builder.Services.AddTransient<QuanQuenBackOfficeMobile.ViewModels.Affiliates.AffiliateAgreementPageViewModel>();
		builder.Services.AddTransient<MainViewModel>();
		builder.Services.AddTransient<CreateStoreUserPasswordViewModel>();
		builder.Services.AddTransient<StoreInformationContractViewModel>();
		builder.Services.AddTransient<ContractPreviewViewModel>();
		builder.Services.AddTransient<ContractPreviewPage>();
		builder.Services.AddTransient<ContractStoreViewModel>();
		builder.Services.AddTransient<ContractStorePage>();
		
		// Register Services
		builder.Services.AddTransient<IContractService, ContractService>();
		builder.Services.AddTransient<IGeocodingService, GeocodingService>();
		
        // Register Views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<AffiliateLoginPage>();
        builder.Services.AddTransient<AffiliateRegisterPage>();
        builder.Services.AddTransient<AffiliateCreateStorePage>();
        builder.Services.AddTransient<AffiliateStoreUserPasswordPage>();
        builder.Services.AddTransient<AffiliateStoreUserAuthenticationPage>();
        builder.Services.AddTransient<AffiliateStoreListPage>();
        builder.Services.AddTransient<QuanQuenBackOfficeMobile.Views.Affiliates.AffiliateAgreementInformationPage>();
        builder.Services.AddTransient<QuanQuenBackOfficeMobile.Views.Affiliates.AffiliateAgreementPreviewPage>();
        builder.Services.AddTransient<QuanQuenBackOfficeMobile.Views.Affiliates.AffiliateAgreementSignaturePage>();
        builder.Services.AddTransient<QuanQuenBackOfficeMobile.Views.Affiliates.AffiliateAgreementPage>();
        builder.Services.AddTransient<QuanQuenBackOfficeMobile.Views.Affiliates.AffiliatePhotoPage>();
        builder.Services.AddTransient<QuanQuenBackOfficeMobile.ViewModels.Affiliates.AffiliatePhotoViewModel>();
        builder.Services.AddTransient<ServicePackagesPage>();
		builder.Services.AddTransient<RegisterPage>();
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<CreateStorePage>();
		builder.Services.AddTransient<StoreInformationContractPage>();
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

		// Removed SkiaSharp Lottie registrations

		builder.Services.AddSingleton<SupportCenterViewModel>();
		builder.Services.AddSingleton<SupportCenterPage>();

		builder.Services.AddTransient<StorePageViewModel>();
		builder.Services.AddTransient<StorePage>();
		builder.Services.AddTransient<StoreDetailViewModel>();
		builder.Services.AddTransient<StoreDetailPage>();
		builder.Services.AddTransient<StoreMapViewModel>();
        builder.Services.AddTransient<StoreMapPage>();
        builder.Services.AddTransient<StoreStatisticPage>();

        builder.Services.AddSingleton<ITokenService, TokenService>();
        builder.Services.AddHttpClient<IServicePackagesService, ServicePackagesService>((sp, client) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var baseUrl = configuration["API:Store:BaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                client.BaseAddress = new Uri(baseUrl);
            }
            client.Timeout = TimeSpan.FromMinutes(2);
        });
        builder.Services.AddHttpClient<ISystemConfigService, SystemConfigService>((sp, client) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var baseUrl = configuration["API:Identity:BaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                client.BaseAddress = new Uri(baseUrl);
            }
            client.Timeout = TimeSpan.FromMinutes(2);
        });

		builder.Services.AddSingleton<GeographicApiService>();

		builder.Services.AddTransient<CreateStoreViewModel>();

		builder.Services.AddTransient<CreateSubStoreViewModel>();

		builder.Services.AddTransient<PhotoStoreUserViewModel>();
		builder.Services.AddTransient<PhotoStoreUser>();
		builder.Services.AddTransient<VideoStoreUserViewModel>();
		builder.Services.AddTransient<VideoStoreUserPage>();

		// Register Scanning QR ViewModel and Page
		builder.Services.AddTransient<ScanningQRPage>();


		// Register Converters
		builder.Services.AddSingleton<Converters.AvatarUrlConverter>();
		builder.Services.AddSingleton<Converters.EnumToBoolConverter>();

		builder.Services.UsePageResolver();

#if DEBUG
		builder.Logging.AddDebug();
#endif
		#if IOS && ANDROID
    builder.ConfigureMauiHandlers(handlers =>
    {
        handlers.AddHandler(typeof(CustomDropdownPicker), typeof(CustomDropdownPickerHandler));
        handlers.AddHandler(typeof(CustomMap), typeof(CustomMapHandler));
        handlers.AddHandler(typeof(Camera.MAUI.CameraView), typeof(Camera.MAUI.CameraViewHandler));
    });
#endif
		return builder.Build();
	}
}
