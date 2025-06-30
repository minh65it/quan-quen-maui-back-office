using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services;
using System.Threading.Tasks;
using System;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System.Linq;

namespace QuanQuenBackOfficeMobile.ViewModels
{
    public partial class StorePageViewModel : ObservableObject
    {
        private readonly IStoreService _storeService;
        private const int DefaultPageSize = 20;

        [ObservableProperty]
        private string searchText;

        [ObservableProperty]
        private ObservableCollection<StoreModel> stores = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isLoadingMore;

        [ObservableProperty]
        private bool hasMoreItems = true;

        private int _currentPage = 1;

        public IAsyncRelayCommand SearchCommand { get; }
        public IRelayCommand<StoreModel> ViewDetailCommand { get; }
        public IRelayCommand<StoreModel> DeleteCommand { get; }
        public IAsyncRelayCommand CreateStoreCommand { get; }

        public StorePageViewModel(IStoreService storeService)
        {
            _storeService = storeService;
            SearchCommand = new AsyncRelayCommand(OnSearchAsync);
            ViewDetailCommand = new RelayCommand<StoreModel>(OnViewDetail);
            DeleteCommand = new RelayCommand<StoreModel>(OnDelete);
            CreateStoreCommand = new AsyncRelayCommand(OnCreateStore);
        }

        private bool _isLoaded = false;
        public async void OnAppearing()
        {
            if (!_isLoaded && !IsLoading)
            {
                _isLoaded = true;
                await LoadStoresAsync();
            }
        }

        private async Task LoadStoresAsync(bool isLoadMore = false)
        {
            try
            {
                if (!isLoadMore)
                {
                    if (IsLoading) return;
                    IsLoading = true;
                    stores.Clear();
                    _currentPage = 1;
                    hasMoreItems = true;
                }
                else
                {
                    if (IsLoadingMore) return;
                    IsLoadingMore = true;
                }

                var response = await _storeService.GetAllStoresAsync(_currentPage, DefaultPageSize, SearchText);
                if (response?.Result != null)
                {
                    var newStores = response.Result.Where(s => !stores.Any(x => x.StoreId == s.StoreId)).ToList();
                    foreach (var store in newStores)
                        stores.Add(store);

                    hasMoreItems = response.Result.Count == DefaultPageSize;
                    if (hasMoreItems)
                    {
                        _currentPage++;
                    }
                }
            }
            catch (Exception)
            {
                // Hiển thị lỗi nếu cần
            }
            finally
            {
                IsLoading = false;
                IsLoadingMore = false;
            }
        }

        public async Task LoadMoreItemsAsync()
        {
            if (!hasMoreItems || IsLoadingMore)
                return;

            await LoadStoresAsync(true);
        }

        private async Task OnSearchAsync()
        {
            _isLoaded = false;
            stores.Clear();
            await LoadStoresAsync();
        }

        public void ResetState()
        {
            _isLoaded = false;
            _currentPage = 1;
            hasMoreItems = true;
            stores.Clear();
        }

        private async void OnViewDetail(StoreModel store)
        {
            var vm = App.Services.GetService<QuanQuenBackOfficeMobile.ViewModels.Stores.StoreDetailViewModel>();
            await vm.LoadStoreDetailAsync(store.StoreId);
            var page = new QuanQuenBackOfficeMobile.Views.Stores.StoreDetailPage(vm);
            App.Current.MainPage.Navigation.PushAsync(page);
        }

        private async void OnDelete(StoreModel store)
        {
        }

        private async Task OnCreateStore()
        {
            var page = App.Services.GetService<CreateStorePage>();
            App.Current.MainPage.Navigation.PushAsync(page);
        }
    }
} 