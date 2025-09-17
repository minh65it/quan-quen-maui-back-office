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

        /// <summary>
        /// Cập nhật thông tin QR Code
        /// </summary>
        /// <param name="qrCode">Mã QR Code</param>
        /// <param name="qrCodeType">Loại QR Code (mặc định "Store")</param>
        /// <param name="identityId">ID của store</param>
        /// <returns>Kết quả cập nhật</returns>
        Task<bool> UpdateQRCodeAsync(string qrCode, string qrCodeType, string identityId);
    }
}
