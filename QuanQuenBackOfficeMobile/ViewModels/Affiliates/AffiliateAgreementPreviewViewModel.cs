using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Views.Affiliates;

namespace QuanQuenBackOfficeMobile.ViewModels.Affiliates
{
    public partial class AffiliateAgreementPreviewViewModel : ObservableObject
    {
        private readonly IContractService _contractService;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isPdfLoaded = false;

        [ObservableProperty]
        private bool hasError = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private HtmlWebViewSource pdfSource = new HtmlWebViewSource();

        public IAsyncRelayCommand RetryCommand { get; }
        public IAsyncRelayCommand SignAgreementCommand { get; }

        public AgreementInfoDto AgreementInfo { get; private set; } = new AgreementInfoDto();

        public AffiliateAgreementPreviewViewModel(IContractService contractService)
        {
            _contractService = contractService;
            RetryCommand = new AsyncRelayCommand(RetryAsync);
            SignAgreementCommand = new AsyncRelayCommand(SignAgreementAsync);
        }

        public void SetApiPdfContent(string pdfBase64Data)
        {
            try
            {
                if (!string.IsNullOrEmpty(pdfBase64Data))
                {
                    var html = CreatePdfJsHtml(pdfBase64Data);
                    PdfSource.Html = html;
                    IsPdfLoaded = true;
                    HasError = false;
                }
                else
                {
                    HasError = true;
                    ErrorMessage = "Không thể tải PDF từ API - Data trống";
                    IsPdfLoaded = false;
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Lỗi hiển thị PDF: {ex.Message}";
                IsPdfLoaded = false;
            }
        }

        public async Task LoadAgreementPreviewAsync(AgreementInfoDto agreementInfo)
        {
            try
            {
                IsBusy = true;
                HasError = false;
                ErrorMessage = string.Empty;
                AgreementInfo = agreementInfo;

                var apiRequest = await _contractService.ConvertToPreviewAgreementRequestAsync(agreementInfo);
                var apiResponse = await _contractService.CallPreviewAgreementApiAsync(apiRequest);
                if (apiResponse.IsSuccess)
                {
                    SetApiPdfContent(apiResponse.Message);
                }
                else
                {
                    HasError = true;
                    ErrorMessage = apiResponse.Message;
                    IsPdfLoaded = false;
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Lỗi khi tải preview: {ex.Message}";
                IsPdfLoaded = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RetryAsync()
        {
            if (AgreementInfo != null)
            {
                await LoadAgreementPreviewAsync(AgreementInfo);
            }
        }

        private async Task SignAgreementAsync()
        {
            try
            {
                var signaturePage = new AffiliateAgreementSignaturePage(AgreementInfo);
                await Application.Current.MainPage.Navigation.PushAsync(signaturePage);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
        }

        private string CreatePdfJsHtml(string pdfBase64Data)
        {
            return $@"<!DOCTYPE html>
<html dir='ltr'>
<head>
  <meta charset='utf-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1, maximum-scale=1'>
  <title>Xem trước thỏa thuận</title>
  <style>
    html, body {{ height:100%; margin:0; }}
    #pdfPagesContainer {{ width:100%; height:100vh; overflow-y:auto; }}
    .page-wrapper {{ margin:10px; }}
    .page-canvas {{ width:100%; height:auto; }}
  </style>
</head>
<body>
  <div id='pdfPagesContainer'></div>
  <script>
    const script = document.createElement('script');
    script.src = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.min.js';
    script.onload = function() {{
      pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js';
      loadPDF();
    }};
    document.head.appendChild(script);

    function sanitizeBase64(b64) {{
      if (!b64) return '';
      if (b64.startsWith('data:')) {{
        const i = b64.indexOf(',');
        if (i > -1) b64 = b64.slice(i + 1);
      }}
      b64 = b64.replace(/\s+/g, '').replace(/-/g, '+').replace(/_/g, '/');
      const mod4 = b64.length % 4; if (mod4) b64 += '='.repeat(4 - mod4);
      return b64;
    }}

    async function loadPDF() {{
      const raw = '{pdfBase64Data}';
      const b64 = sanitizeBase64(raw);
      const dataUrl = 'data:application/pdf;base64,' + b64;
      const resp = await fetch(dataUrl);
      const bytes = new Uint8Array(await resp.arrayBuffer());
      const pdf = await pdfjsLib.getDocument({{ data: bytes }}).promise;
      const container = document.getElementById('pdfPagesContainer');
      for (let p = 1; p <= pdf.numPages; p++) {{
        const page = await pdf.getPage(p);
        const viewport = page.getViewport({{ scale: 1.2 }});
        const canvas = document.createElement('canvas');
        canvas.width = viewport.width;
        canvas.height = viewport.height;
        canvas.className = 'page-canvas';
        const ctx = canvas.getContext('2d');
        const wrapper = document.createElement('div');
        wrapper.className = 'page-wrapper';
        wrapper.appendChild(canvas);
        container.appendChild(wrapper);
        await page.render({{ canvasContext: ctx, viewport }}).promise;
      }}
    }}
  </script>
</body>
</html>";
        }
    }
}


