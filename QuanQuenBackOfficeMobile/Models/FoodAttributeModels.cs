using System.Collections.Generic;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace QuanQuenBackOfficeMobile.Models
{
    public partial class AttributeCategory : ObservableObject
    {
        [ObservableProperty]
        [JsonPropertyName("id")]
        private string? id;

        [ObservableProperty]
        [JsonPropertyName("name")]
        private string? name;

        [ObservableProperty]
        [JsonPropertyName("description")]
        private string? description;

        [ObservableProperty]
        [JsonPropertyName("position")]
        private int? position;

        [ObservableProperty]
        [JsonPropertyName("urlImage")]
        private string? urlImage;

        [ObservableProperty]
        [JsonPropertyName("colorCode")]
        private string? colorCode;

        [ObservableProperty]
        [JsonPropertyName("attributes")]
        private List<FoodAttribute> attributes = new();

        [ObservableProperty]
        private bool isSelected;
    }

    public partial class FoodAttribute : ObservableObject
    {
        [ObservableProperty]
        [JsonPropertyName("id")]
        private string? id;

        [ObservableProperty]
        [JsonPropertyName("name")]
        private string? name;

        [ObservableProperty]
        [JsonPropertyName("description")]
        private string? description;

        [ObservableProperty]
        [JsonPropertyName("position")]
        private int? position;

        [ObservableProperty]
        [JsonPropertyName("urlImage")]
        private string? urlImage;

        [ObservableProperty]
        [JsonPropertyName("colorCode")]
        private string? colorCode;

        [ObservableProperty]
        [JsonPropertyName("categoryId")]
        private string? categoryId;

        [ObservableProperty]
        private bool isSelected;
    }

    public class AttributeCategoryListResponse
    {
        [JsonPropertyName("pagination")]
        public PaginationInfo? Pagination { get; set; }

        [JsonPropertyName("result")]
        public List<AttributeCategory> Result { get; set; } = new();
    }

    public class FoodAttributeListResponse
    {
        [JsonPropertyName("pagination")]
        public PaginationInfo? Pagination { get; set; }

        [JsonPropertyName("result")]
        public List<FoodAttribute> Result { get; set; } = new();
    }
}

