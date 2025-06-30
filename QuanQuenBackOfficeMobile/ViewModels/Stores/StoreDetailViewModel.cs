using CommunityToolkit.Mvvm.ComponentModel;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System.Threading.Tasks;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores
{
    public partial class StoreDetailViewModel : ObservableObject
    {
        private readonly IStoreService _storeService;

        [ObservableProperty]
        private StoreDetailModel storeDetail;

        public StoreDetailViewModel(IStoreService storeService)
        {
            _storeService = storeService;
        }

        public async Task LoadStoreDetailAsync(string storeId)
        {
            try
            {
                StoreDetail = await _storeService.GetStoreDetailAsync(storeId);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Không thể tải chi tiết nhà hàng: " + ex.Message, "OK");
            }
        }
    }
} 