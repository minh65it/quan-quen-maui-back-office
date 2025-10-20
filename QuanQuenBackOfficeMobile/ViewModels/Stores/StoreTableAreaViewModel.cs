using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Views.Stores;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores
{
    public partial class StoreTableAreaViewModel : ObservableObject
    {
        private readonly ITableAreaService _tableAreaService;

        [ObservableProperty]
        private string storeId = string.Empty;

        [ObservableProperty]
        private ObservableCollection<TableAreaItem> items = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string newAreaName = string.Empty;

        [ObservableProperty]
        private string newAreaDescription = string.Empty;

        [ObservableProperty]
        private int newOrder = 0;

        [ObservableProperty]
        private bool isCreating;

        [ObservableProperty]
        private string editingAreaId = string.Empty;

        [ObservableProperty]
        private string editingAreaName = string.Empty;

        [ObservableProperty]
        private string editingAreaDescription = string.Empty;

        [ObservableProperty]
        private int editingOrder = 0;

        [ObservableProperty]
        private bool isUpdating;

        [ObservableProperty]
        private string currentEditingItemId = string.Empty;


        public StoreTableAreaViewModel(ITableAreaService tableAreaService)
        {
            _tableAreaService = tableAreaService;
        }

        public void Initialize(string storeId)
        {
            StoreId = storeId;
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            if (string.IsNullOrWhiteSpace(StoreId)) return;
            try
            {
                IsLoading = true;
                // Reset edit mode khi reload
                EditingAreaId = string.Empty;
                CurrentEditingItemId = string.Empty;
                var data = await _tableAreaService.GetTableAreasAsync(StoreId);
                Items = new ObservableCollection<TableAreaItem>(data);
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
        public async Task ViewTables(TableAreaItem areaItem)
        {
            try
            {
                if (areaItem?.TableAreaId == null)
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi", "Không tìm thấy thông tin khu vực", "OK");
                    return;
                }

                var vm = App.Services.GetService<StoreTableListViewModel>();
                if (vm == null)
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi", "Không thể khởi tạo ViewModel danh sách bàn", "OK");
                    return;
                }
                var page = new StoreTableListPage(vm, StoreId, areaItem.TableAreaId, areaItem.AreaName ?? "Khu vực");
                await App.Current.MainPage.Navigation.PushAsync(page);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể mở trang danh sách bàn: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task CreateTableArea()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewAreaName))
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập tên khu vực", "OK");
                    return;
                }

                IsCreating = true;
                var response = await _tableAreaService.CreateTableAreaAsync(
                    StoreId, 
                    NewAreaName.Trim(), 
                    string.IsNullOrWhiteSpace(NewAreaDescription) ? null : NewAreaDescription.Trim(), 
                    NewOrder
                );

                if (response.IsSuccess)
                {
                    await App.Current.MainPage.DisplayAlert("Thành công", "Đã tạo khu vực bàn mới", "OK");
                    
                    // Reset form
                    NewAreaName = string.Empty;
                    NewAreaDescription = string.Empty;
                    NewOrder = 0;
                    
                    // Reload danh sách để lấy ID thật từ server
                    await LoadAsync();
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi", response.Message ?? "Tạo khu vực thất bại", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể tạo khu vực: {ex.Message}", "OK");
            }
            finally
            {
                IsCreating = false;
            }
        }

        [RelayCommand]
        public void StartEdit(TableAreaItem areaItem)
        {
            if (string.IsNullOrEmpty(value: areaItem.TableAreaId))
                return;
                
            EditingAreaId = areaItem.TableAreaId;
            CurrentEditingItemId = areaItem.TableAreaId;
            EditingAreaName = areaItem.AreaName ?? string.Empty;
            EditingAreaDescription = areaItem.AreaDescription ?? string.Empty;
            EditingOrder = areaItem.Order ?? 0;
        }

        [RelayCommand]
        public void CancelEdit()
        {
            EditingAreaId = string.Empty;
            CurrentEditingItemId = string.Empty;
            EditingAreaName = string.Empty;
            EditingAreaDescription = string.Empty;
            EditingOrder = 0;
        }

        [RelayCommand]
        public async Task UpdateTableArea()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EditingAreaName))
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập tên khu vực", "OK");
                    return;
                }

                IsUpdating = true;
                var response = await _tableAreaService.UpdateTableAreaAsync(
                    StoreId,
                    EditingAreaId,
                    EditingAreaName.Trim(),
                    string.IsNullOrWhiteSpace(EditingAreaDescription) ? null : EditingAreaDescription.Trim(),
                    EditingOrder
                );

                if (response.IsSuccess)
                {
                    await App.Current.MainPage.DisplayAlert("Thành công", "Đã cập nhật khu vực bàn", "OK");
                    
                    // Cập nhật trực tiếp item trong collection thay vì reload toàn bộ
                    var itemToUpdate = Items.FirstOrDefault(x => x.TableAreaId == EditingAreaId);
                    if (itemToUpdate != null)
                    {
                        itemToUpdate.AreaName = EditingAreaName.Trim();
                        itemToUpdate.AreaDescription = string.IsNullOrWhiteSpace(EditingAreaDescription) ? null : EditingAreaDescription.Trim();
                        itemToUpdate.Order = EditingOrder;
                    }
                    
                    // Reset edit form
                    CancelEdit();
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi", response.Message ?? "Cập nhật khu vực thất bại", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể cập nhật khu vực: {ex.Message}", "OK");
            }
            finally
            {
                IsUpdating = false;
            }
        }
    }
}


