using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.ViewModels.Stores;

namespace QuanQuenBackOfficeMobile.Services.Interfaces
{
    public interface IContractService
    {

        // API methods for PreviewContract
        Task<PreviewContractResponse> CallPreviewContractApiAsync(PreviewContractRequest request);
        Task<PreviewContractRequest> ConvertToPreviewContractRequestAsync(StoreInformationContractViewModel viewModel);
        
        // API method for PreviewContract with signatures
        Task<PreviewContractResponse> CallPreviewContractApiWithSignaturesAsync(PreviewContractRequest request, byte[] contractSignature, byte[] appendixSignature);

        // API methods for PreviewCooperationAgreement
        Task<PreviewAgreementResponse> CallPreviewAgreementApiAsync(PreviewAgreementRequest request);
        Task<PreviewAgreementRequest> ConvertToPreviewAgreementRequestAsync(AgreementInfoDto agreementInfo);
        Task<PreviewAgreementResponse> CallPreviewAgreementApiWithSignatureAsync(PreviewAgreementRequest request, byte[] signatureFile, byte[] qrCodeFile);

    }
} 