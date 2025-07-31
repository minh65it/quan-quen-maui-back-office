using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using QuanQuenBackOfficeMobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System.Threading.Tasks;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores
{
    public partial class StoreInformationContractViewModel : ObservableObject
    {
        private readonly IContractService _contractService;

        public List<string> BusinessTypeList { get; } = new List<string> { "Tổ chức", "Hộ kinh doanh","Tư Nhân" };

        [ObservableProperty]
        private string selectedBusinessType;



        [ObservableProperty]
        private bool isBusy;

        // Main Contract - Tổ chức
        [ObservableProperty]
        private string companyName;

        [ObservableProperty]
        private string storeName;

        [ObservableProperty]
        private string headquartersAddress;

        [ObservableProperty]
        private string businessLicenseNumber;

        [ObservableProperty]
        private string businessLicenseDate;

        [ObservableProperty]
        private string businessLicensePlace;

        [ObservableProperty]
        private string representativeName;

        [ObservableProperty]
        private string representativePosition;

        [ObservableProperty]
        private string representativeIdNumber;

        [ObservableProperty]
        private string representativeIdDate;

        [ObservableProperty]
        private string representativeIdPlace;

        [ObservableProperty]
        private string representativePermanentAddress;

        [ObservableProperty]
        private string representativeTemporaryAddress;

        [ObservableProperty]
        private string storeHeadOfficeAddress;

        [ObservableProperty]
        private string phoneNumber;

        [ObservableProperty]
        private string bankAccountNumber;

        [ObservableProperty]
        private string bankAccountHolder;

        [ObservableProperty]
        private string bankName;

        // Main Contract - Hộ kinh doanh
        [ObservableProperty]
        private string ownerName;

        [ObservableProperty]
        private string ownerGender;

        [ObservableProperty]
        private string ownerIdNumber;

        [ObservableProperty]
        private string ownerIdDate;

        [ObservableProperty]
        private string ownerIdPlace;

        [ObservableProperty]
        private string ownerPermanentAddress;

        [ObservableProperty]
        private string ownerTemporaryAddress;



        public ICommand NextCommand { get; }
        public ICommand PreviewCommand { get; }


        public StoreInformationContractViewModel(IContractService contractService)
        {
            _contractService = contractService;
            NextCommand = new AsyncRelayCommand(OnNextClicked);
            PreviewCommand = new AsyncRelayCommand(OnPreviewClicked);
        }



        private async Task OnNextClicked()
        {
            try
            {
                IsBusy = true;
                // Lưu thông tin hợp đồng vào database hoặc local storage
                // TODO: Implement save logic
                
                // Chuyển đến trang vẽ chữ ký
                await Application.Current.MainPage.Navigation.PushAsync(new QuanQuenBackOfficeMobile.Views.DrawingExamplePage());
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task OnPreviewClicked()
        {
            try
            {
                IsBusy = true;

                // Convert ViewModel data to API request
                var apiRequest = await _contractService.ConvertToPreviewContractRequestAsync(this);

                // Call PreviewContract API
                var apiResponse = await _contractService.CallPreviewContractApiAsync(apiRequest);

                if (apiResponse.IsSuccess)
                {
                    // Chuyển đến trang preview với PDF từ API
                    var previewViewModel = new ContractPreviewViewModel();
                    
                    // Truyền PDF base64 từ API response message
                    previewViewModel.SetApiPdfContent(apiResponse.GetPdfDataUrl());
                    
                    // Pass contract data để có thể navigate tới signing page
                    var previewPage = new QuanQuenBackOfficeMobile.Views.Stores.ContractPreviewPage(previewViewModel, this);
                    await Application.Current.MainPage.Navigation.PushAsync(previewPage);
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi API", apiResponse.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }


    }
} 