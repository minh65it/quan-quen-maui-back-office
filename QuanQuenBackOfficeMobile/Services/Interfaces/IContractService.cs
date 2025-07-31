using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.ViewModels.Stores;

namespace QuanQuenBackOfficeMobile.Services.Interfaces
{
    public interface IContractService
    {

        // API methods for PreviewContract
        Task<PreviewContractResponse> CallPreviewContractApiAsync(PreviewContractRequest request);
        Task<PreviewContractRequest> ConvertToPreviewContractRequestAsync(StoreInformationContractViewModel viewModel);

    }
} 