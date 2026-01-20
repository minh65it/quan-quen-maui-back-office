using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace QuanQuenBackOfficeMobile.Models
{
    public partial class FoodFilterItem : ObservableObject
    {
        [ObservableProperty]
        [JsonPropertyName("filterFoodId")]
        private string? filterFoodId;

        [ObservableProperty]
        [JsonPropertyName("name")]
        private string? name;

        [ObservableProperty]
        [JsonPropertyName("value")]
        private string? value;

        [ObservableProperty]
        [JsonPropertyName("state")]
        private string? state;

        [ObservableProperty]
        [JsonPropertyName("position")]
        private int? position;

        [ObservableProperty]
        [JsonPropertyName("urlImage")]
        private string? urlImage;

        [ObservableProperty]
        [JsonPropertyName("storeId")]
        private string? storeId;

        [ObservableProperty]
        [JsonPropertyName("createDate")]
        private DateTime? createDate;

        [ObservableProperty]
        [JsonPropertyName("createBy")]
        private string? createBy;

        [ObservableProperty]
        [JsonPropertyName("updateDate")]
        private DateTime? updateDate;

        [ObservableProperty]
        [JsonPropertyName("updateBy")]
        private string? updateBy;

        [ObservableProperty]
        [JsonPropertyName("isDelete")]
        private bool? isDelete;
    }

    public class FoodFilterListResponse
    {
        [JsonPropertyName("pagination")] public PaginationInfo? Pagination { get; set; }
        [JsonPropertyName("result")] public List<FoodFilterItem> Result { get; set; } = new();
    }
}

