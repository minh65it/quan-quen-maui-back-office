using System.Text.Json.Serialization;

namespace QuanQuenBackOfficeMobile.Models
{
    public class QRCodeRequest
    {
        [JsonPropertyName("qrText")]
        public string QRText { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class QRCodeData
    {
        [JsonPropertyName("diaChi")]
        public string DiaChi { get; set; } = string.Empty;

        [JsonPropertyName("toaDo")]
        public Coordinates ToaDo { get; set; } = new();

        [JsonPropertyName("tenNguoiDaiDien")]
        public string TenNguoiDaiDien { get; set; } = string.Empty;

        [JsonPropertyName("agentInfo")]
        public AgentInfo AgentInfo { get; set; } = new();

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class Coordinates
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }

    public class AgentInfo
    {
        [JsonPropertyName("ten")]
        public string Ten { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    public class QRCodeResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("qrCodeImage")]
        public string QRCodeImage { get; set; } = string.Empty; // URL của image hoặc base64 string
    }
}
