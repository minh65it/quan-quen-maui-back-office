using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.ViewModels.Stores;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores;

public partial class StoreUserAuthenticationViewModel : ObservableObject
{
    [ObservableProperty]
    private string otpCode;

    [ObservableProperty]
    private string countdownText = "25s";

    [ObservableProperty]
    private bool canResend = false;

    [ObservableProperty]
    private string email;


    private readonly IStoreUserService _storeUserService;

    public ICommand VerifyOtpCommand { get; }
    public ICommand ResendOtpCommand { get; }

    private int countdown = 25;
    private System.Timers.Timer timer;

    private static HashSet<string> _sentOtpEmails = new();

    public StoreUserAuthenticationViewModel(IStoreUserService storeUserService, string email)
    {
        _storeUserService = storeUserService;
        Email = email;
        VerifyOtpCommand = new AsyncRelayCommand(VerifyOtpAsync);
        ResendOtpCommand = new AsyncRelayCommand(ResendOtpAsync, () => CanResend);
        StartCountdown();
    }

    private void StartCountdown()
    {
        CanResend = false;
        countdown = 25;
        CountdownText = $"{countdown}s";
        timer = new System.Timers.Timer(1000);
        timer.Elapsed += (s, e) => {
            countdown--;
            CountdownText = $"{countdown}s";
            if (countdown <= 0)
            {
                timer.Stop();
                CanResend = true;
                CountdownText = "0s";
                (ResendOtpCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
            }
        };
        timer.Start();
    }

    private async Task VerifyOtpAsync()
    {
        if (string.IsNullOrWhiteSpace(OtpCode) || string.IsNullOrWhiteSpace(Email))
        {
            await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập mã OTP", "OK");
            return;
        }

        var response = await _storeUserService.VerifyCodeEmailAsync(Email, OtpCode);
        if (response.IsSuccess)
        {
            await App.Current.MainPage.DisplayAlert("Thành công", "Xác thực thành công!", "OK");
            var contractService = App.Services.GetService<IContractService>();
            var viewModel = new StoreInformationContractViewModel(contractService);
            await Application.Current.MainPage.Navigation.PushAsync(new QuanQuenBackOfficeMobile.Views.Stores.StoreInformationContractPage(viewModel));
        }
        else
        {
            await App.Current.MainPage.DisplayAlert("Lỗi", response.Message ?? "Mã xác thực không chính xác", "OK");
        }
    }

    private async Task ResendOtpAsync()
    {
        if (!CanResend)
        {
            return;
        }

        await SendOtpAsync();
        StartCountdown();
        await App.Current.MainPage.DisplayAlert("OTP", "Đã gửi lại mã OTP!", "OK");
    }

    private async Task SendOtpAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            return;
        }

        var response = await _storeUserService.SendCodeEmailAsync(Email);
        if (!response.IsSuccess)
        {
            await App.Current.MainPage.DisplayAlert("Thông Báo", response.Message, "OK");
        }
    }
} 