using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.Services.Extensions
{
    public static class QRCodeExtensions
    {
        /// <summary>
        /// Tạo QR Code cho store với thông tin cơ bản
        /// </summary>
        public static async Task<QRCodeResponse> GenerateStoreQRCodeAsync(
            this IQRCodeService qrCodeService,
            string address,
            double latitude,
            double longitude,
            string representativeName,
            string agentName,
            string agentId)
        {
            var storeData = new QRCodeData
            {
                DiaChi = address,
                ToaDo = new Coordinates
                {
                    Latitude = latitude,
                    Longitude = longitude
                },
                TenNguoiDaiDien = representativeName,
                AgentInfo = new AgentInfo
                {
                    Ten = agentName,
                    Id = agentId
                },
                Timestamp = DateTime.UtcNow
            };

            return await qrCodeService.GenerateStoreQRCodeAsync(storeData, storeData.AgentInfo);
        }
    }
}
