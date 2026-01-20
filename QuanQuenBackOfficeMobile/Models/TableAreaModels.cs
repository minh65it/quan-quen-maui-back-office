using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace QuanQuenBackOfficeMobile.Models
{
    public partial class TableAreaItem : ObservableObject
    {
        [ObservableProperty]
        [JsonPropertyName("tableAreaId")]
        private string? tableAreaId;

        [ObservableProperty]
        [JsonPropertyName("storeId")]
        private string? storeId;

        [ObservableProperty]
        [JsonPropertyName("areaName")]
        private string? areaName;

        [ObservableProperty]
        [JsonPropertyName("areaDescription")]
        private string? areaDescription;

        [ObservableProperty]
        [JsonPropertyName("order")]
        private int? order;

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

    public class TableAreaListResponse
    {
        [JsonPropertyName("pagination")] public PaginationInfo? Pagination { get; set; }
        [JsonPropertyName("result")] public List<TableAreaItem> Result { get; set; } = new();
    }

    public partial class TableItem : ObservableObject
    {
        [ObservableProperty]
        [JsonPropertyName("tableId")]
        private string? tableId;

        [ObservableProperty]
        [JsonPropertyName("storeId")]
        private string? storeId;

        [ObservableProperty]
        [JsonPropertyName("tableName")]
        private string? tableName;

        [ObservableProperty]
        [JsonPropertyName("tableNumber")]
        private int? tableNumber;

        [ObservableProperty]
        [JsonPropertyName("capacity")]
        private int? capacity;

        [ObservableProperty]
        [JsonPropertyName("tableStatus")]
        private int? tableStatus;

        [ObservableProperty]
        [JsonPropertyName("tableAreaId")]
        private string? tableAreaId;

        [ObservableProperty]
        [JsonPropertyName("urlQrCode")]
        private string? urlQrCode;

        [ObservableProperty]
        [JsonPropertyName("userId")]
        private string? userId;

        [ObservableProperty]
        [JsonPropertyName("userName")]
        private string? userName;

        [ObservableProperty]
        [JsonPropertyName("tableDescription")]
        private string? tableDescription;

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

    public class TableListResponse
    {
        [JsonPropertyName("pagination")] public PaginationInfo? Pagination { get; set; }
        [JsonPropertyName("result")] public List<TableItem> Result { get; set; } = new();
    }
}


