using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores
{
    public partial class StoreTableListViewModel : ObservableObject
    {
        private readonly ITableService _tableService;

        [ObservableProperty]
        private string storeId = string.Empty;

        [ObservableProperty]
        private string tableAreaId = string.Empty;

        [ObservableProperty]
        private string areaName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<TableItem> items = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string newTableName = string.Empty;

        [ObservableProperty]
        private string newTableDescription = string.Empty;

        [ObservableProperty]
        private int newTableNumber = 0;

        [ObservableProperty]
        private int newCapacity = 2;

        [ObservableProperty]
        private int newTableStatus = 0;

        [ObservableProperty]
        private bool isCreating;

        [ObservableProperty]
        private string editingTableId = string.Empty;

        [ObservableProperty]
        private string editingTableName = string.Empty;

        [ObservableProperty]
        private string editingTableDescription = string.Empty;

        [ObservableProperty]
        private int editingTableNumber = 0;

        [ObservableProperty]
        private int editingCapacity = 2;

        [ObservableProperty]
        private int editingTableStatus = 0;

        [ObservableProperty]
        private bool isUpdating;

        [ObservableProperty]
        private string currentEditingItemId = string.Empty;

        public StoreTableListViewModel(ITableService tableService)
        {
            _tableService = tableService;
        }

        public void Initialize(string storeId, string tableAreaId, string areaName)
        {
            StoreId = storeId;
            TableAreaId = tableAreaId;
            AreaName = areaName;
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            if (string.IsNullOrWhiteSpace(StoreId) || string.IsNullOrWhiteSpace(TableAreaId)) return;
            try
            {
                IsLoading = true;
                // Reset edit mode khi reload
                EditingTableId = string.Empty;
                CurrentEditingItemId = string.Empty;
                var data = await _tableService.GetTablesAsync(StoreId, TableAreaId);
                Items = new ObservableCollection<TableItem>(data);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task CreateTable()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewTableName))
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập tên bàn", "OK");
                    return;
                }

                IsCreating = true;
                var response = await _tableService.CreateTableAsync(
                    StoreId,
                    TableAreaId,
                    NewTableNumber,
                    NewTableName.Trim(),
                    NewTableStatus,
                    NewCapacity,
                    string.IsNullOrWhiteSpace(NewTableDescription) ? null : NewTableDescription.Trim()
                );

                if (response.IsSuccess)
                {
                    await App.Current.MainPage.DisplayAlert("Thành công", "Đã tạo bàn mới", "OK");
                    
                    // Reset form
                    NewTableName = string.Empty;
                    NewTableDescription = string.Empty;
                    NewTableNumber = 0;
                    NewCapacity = 2;
                    NewTableStatus = 0;
                    
                    // Reload danh sách để lấy ID thật từ server
                    await LoadAsync();
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi", response.Message ?? "Tạo bàn thất bại", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể tạo bàn: {ex.Message}", "OK");
            }
            finally
            {
                IsCreating = false;
            }
        }

        [RelayCommand]
        public void StartEdit(TableItem tableItem)
        {
            if (string.IsNullOrEmpty(tableItem.TableId))
            {
                // Không thể edit item không có ID
                return;
            }
            
            EditingTableId = tableItem.TableId;
            CurrentEditingItemId = tableItem.TableId;
            EditingTableName = tableItem.TableName ?? string.Empty;
            EditingTableDescription = tableItem.TableDescription ?? string.Empty;
            EditingTableNumber = tableItem.TableNumber ?? 0;
            EditingCapacity = tableItem.Capacity ?? 2;
            EditingTableStatus = tableItem.TableStatus ?? 0;
        }

        [RelayCommand]
        public void CancelEdit()
        {
            EditingTableId = string.Empty;
            CurrentEditingItemId = string.Empty;
            EditingTableName = string.Empty;
            EditingTableDescription = string.Empty;
            EditingTableNumber = 0;
            EditingCapacity = 2;
            EditingTableStatus = 0;
        }

        [RelayCommand]
        public async Task UpdateTable()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EditingTableName))
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập tên bàn", "OK");
                    return;
                }

                IsUpdating = true;
                var response = await _tableService.UpdateTableAsync(
                    EditingTableId,
                    StoreId,
                    TableAreaId,
                    EditingTableNumber,
                    EditingTableName.Trim(),
                    EditingTableStatus,
                    EditingCapacity,
                    string.IsNullOrWhiteSpace(EditingTableDescription) ? null : EditingTableDescription.Trim()
                );

                if (response.IsSuccess)
                {
                    await App.Current.MainPage.DisplayAlert("Thành công", "Đã cập nhật bàn", "OK");
                    
                    // Cập nhật trực tiếp item trong collection thay vì reload toàn bộ
                    var itemToUpdate = Items.FirstOrDefault(x => x.TableId == EditingTableId);
                    if (itemToUpdate != null)
                    {
                        itemToUpdate.TableName = EditingTableName.Trim();
                        itemToUpdate.TableDescription = string.IsNullOrWhiteSpace(EditingTableDescription) ? null : EditingTableDescription.Trim();
                        itemToUpdate.TableNumber = EditingTableNumber;
                        itemToUpdate.TableStatus = EditingTableStatus;
                        itemToUpdate.Capacity = EditingCapacity;
                    }
                    
                    // Reset edit form
                    CancelEdit();
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi", response.Message ?? "Cập nhật bàn thất bại", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể cập nhật bàn: {ex.Message}", "OK");
            }
            finally
            {
                IsUpdating = false;
            }
        }
    }
}
