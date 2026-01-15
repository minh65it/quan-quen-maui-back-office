using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QuanQuenBackOfficeMobile.Models
{
    public partial class CategoryMasterModel : ObservableObject
    {
        [ObservableProperty]
        [JsonPropertyName("id")]
        private string? id;

        [ObservableProperty]
        [JsonPropertyName("name")]
        private string? name;

        [ObservableProperty]
        [JsonPropertyName("value")]
        private string? value;

        [ObservableProperty]
        [JsonPropertyName("description")]
        private string? description;

        [ObservableProperty]
        [JsonPropertyName("backgroundColor")]
        private string? backgroundColor;

        [ObservableProperty]
        [JsonPropertyName("textColor")]
        private string? textColor;

        [ObservableProperty]
        [JsonPropertyName("url")]
        private string? url;

        [ObservableProperty]
        [JsonPropertyName("position")]
        private int position;
    }

    public class CategoryMasterData
    {
        [JsonPropertyName("pagination")]
        public PaginationInfo? Pagination { get; set; }

        [JsonPropertyName("result")]
        public List<CategoryMasterModel> Result { get; set; } = new();
    }

    public class CategoryMasterResponse
    {
        [JsonPropertyName("data")]
        public CategoryMasterData? Data { get; set; }
    }
}
