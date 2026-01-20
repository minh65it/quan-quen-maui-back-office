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

        /// <summary>
        /// Quét QRCode và tạo bàn, đồng thời gán QRCode cho bàn
        /// </summary>
        /// <param name="qrCode">Giá trị QRCode đã quét</param>
        /// <param name="storeId">ID cửa hàng</param>
        /// <param name="tableAreaId">Khu vực bàn (tùy chọn)</param>
        /// <param name="tableName">Tên bàn (tùy chọn)</param>
        /// <param name="tableNumber">Số bàn (tùy chọn)</param>
        /// <param name="capacity">Sức chứa (tùy chọn)</param>
        /// <param name="tableDescription">Mô tả bàn (tùy chọn)</param>
        /// <param name="userId">Người phụ trách (tùy chọn)</param>
        /// <param name="userName">Tên người phụ trách (tùy chọn)</param>
        /// <returns>Thành công/Thất bại</returns>
        Task<bool> ScanQRCodeAndCreateTableAsync(
            string qrCode,
            string storeId,
            string? tableAreaId = null,
            string? tableName = null,
            string? tableNumber = null,
            int? capacity = null,
            string? tableDescription = null,
            string? userId = null,
            string? userName = null);
    }
}
