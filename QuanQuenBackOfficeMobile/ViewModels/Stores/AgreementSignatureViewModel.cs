using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores
{
    public partial class AgreementSignatureViewModel : ObservableObject
    {
        private readonly IContractService _contractService;
        private readonly AgreementInfoDto _agreementInfo;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private ObservableCollection<IDrawingLine> signatureLines = new();

        public IAsyncRelayCommand ClearSignatureCommand { get; }
        public IAsyncRelayCommand SaveAgreementWithSignatureCommand { get; }

        public AgreementSignatureViewModel(IContractService contractService, AgreementInfoDto agreementInfo)
        {
            _contractService = contractService;
            _agreementInfo = agreementInfo;
            
            ClearSignatureCommand = new AsyncRelayCommand(ClearSignatureAsync);
            SaveAgreementWithSignatureCommand = new AsyncRelayCommand<byte[]>(SaveAgreementWithSignatureAsync);
        }

        /// <summary>
        /// Xóa chữ ký
        /// </summary>
        private async Task ClearSignatureAsync()
        {
            try
            {
                SignatureLines.Clear();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Lưu thỏa thuận với chữ ký
        /// </summary>
        public async Task<PreviewAgreementResponse> SaveAgreementWithSignatureAsync(byte[] signatureBytes)
        {
            try
            {
                IsBusy = true;

                // Convert to API request
                var apiRequest = await _contractService.ConvertToPreviewAgreementRequestAsync(_agreementInfo);

                // Call API với signature
                var apiResponse = await _contractService.CallPreviewAgreementApiWithSignatureAsync(apiRequest, signatureBytes);

                return apiResponse;
            }
            catch (Exception ex)
            {
                return new PreviewAgreementResponse
                {
                    IsSuccess = false,
                    Message = $"Lỗi khi lưu thỏa thuận: {ex.Message}"
                };
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
