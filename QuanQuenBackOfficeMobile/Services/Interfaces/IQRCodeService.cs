using QuanQuenBackOfficeMobile.Models;

namespace QuanQuenBackOfficeMobile.Services.Interfaces
{
    public interface IQRCodeService
    {
        /// <summary>
        /// Tạo QR Code cho store với thông tin đầy đủ
        /// </summary>
        /// <param name="storeData">Thông tin store để tạo QR Code</param>
        /// <param name="agentInfo">Thông tin agent tạo QR Code</param>
        /// <returns>QR Code response chứa hình ảnh</returns>
        Task<QRCodeResponse> GenerateStoreQRCodeAsync(QRCodeData storeData, AgentInfo agentInfo);
    }
}
