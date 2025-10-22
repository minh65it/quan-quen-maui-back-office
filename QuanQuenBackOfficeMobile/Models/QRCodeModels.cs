using System.Text.Json.Serialization;

namespace QuanQuenBackOfficeMobile.Models
{
    public class QRCodeData
    {
        [JsonPropertyName("Địa Chỉ")]
        public string DiaChi { get; set; } = string.Empty;

        [JsonPropertyName("Tọa Độ")]
        public Coordinates ToaDo { get; set; } = new();

        [JsonPropertyName("Tên Đại Diện Nhà Hàng")]
        public string TenNguoiDaiDien { get; set; } = string.Empty;

        [JsonPropertyName("Thông Tin Sale")]
        public AgentInfo AgentInfo { get; set; } = new();

        [JsonPropertyName("Thời Gian")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class Coordinates
    {
        [JsonPropertyName("vĩ độ")]
        public double Latitude { get; set; }

        [JsonPropertyName("kinh độ")]
        public double Longitude { get; set; }
    }

    public class AgentInfo
    {
        [JsonPropertyName("Tên")]
        public string Ten { get; set; } = string.Empty;

        [JsonPropertyName("Id")]
        public string Id { get; set; } = string.Empty;
    }

    public class QRCodeResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("qrCodeImage")]
        public string QRCodeImage { get; set; } = string.Empty; 
    }

    public class UpdateQRCodeRequest
    {
        [JsonPropertyName("identityId")]
        public string? IdentityId { get; set; }

        [JsonPropertyName("qrCodeType")]
        public string? QRCodeType { get; set; }

        [JsonPropertyName("qRcode")]
        public string? QRCode { get; set; }

        [JsonPropertyName("qrFormat")]
        public int QRFormat { get; set; }

        [JsonPropertyName("jsonContent")]
        public JsonContentData? JsonContent { get; set; }
    }

    public class JsonContentData
    {
        [JsonPropertyName("objectId")]
        public string[]? ObjectId { get; set; }

        [JsonPropertyName("userId")]
        public string[]? UserId { get; set; }
    }
}
