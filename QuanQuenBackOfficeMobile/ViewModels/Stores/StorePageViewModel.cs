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

        private readonly IAiChatbotService _aiChatbotService;

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

        [ObservableProperty]
        private string paginationInfo = "";

        [ObservableProperty]
        private int totalCount = 0;

        [ObservableProperty]
        private int totalPage = 0;

        [ObservableProperty]
        private int currentPage = 1;

        private int _currentPage = 1;

        public IAsyncRelayCommand SearchCommand { get; }
        public IRelayCommand<StoreModel> ViewDetailCommand { get; }
        public IRelayCommand<StoreModel> DeleteCommand { get; }
        public IAsyncRelayCommand CreateStoreCommand { get; }

        [ObservableProperty]
        private bool isChatbotVisible;

        [ObservableProperty]
        private ObservableCollection<ChatMessage> chatMessages = new();

        [ObservableProperty]
        private string chatbotInput = string.Empty;

        public IRelayCommand ToggleChatbotCommand { get; }
        public IAsyncRelayCommand SendChatbotCommand { get; }

        public StorePageViewModel(IStoreService storeService, IAiChatbotService aiChatbotService)
        {
            _storeService = storeService;
            _aiChatbotService = aiChatbotService;
            SearchCommand = new AsyncRelayCommand(OnSearchAsync);
            ViewDetailCommand = new RelayCommand<StoreModel>(OnViewDetail);
            DeleteCommand = new RelayCommand<StoreModel>(OnDelete);
            CreateStoreCommand = new AsyncRelayCommand(OnCreateStore);

            ToggleChatbotCommand = new RelayCommand(() => IsChatbotVisible = !IsChatbotVisible);
            SendChatbotCommand = new AsyncRelayCommand(SendChatbotAsync, CanSendChatbot);
        }

        private bool CanSendChatbot()
        {
            return !string.IsNullOrWhiteSpace(ChatbotInput);
        }

        partial void OnChatbotInputChanged(string value)
        {
            SendChatbotCommand.NotifyCanExecuteChanged();
        }

        private async Task SendChatbotAsync()
        {
            try
            {
                var msg = ChatbotInput?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(msg)) return;

                // Thêm tin nhắn của user
                var userMessage = new ChatMessage
                {
                    Content = msg,
                    IsFromUser = true,
                    Timestamp = DateTime.Now
                };
                ChatMessages.Add(userMessage);

                // Xóa input
                ChatbotInput = string.Empty;

                // Gọi API và thêm phản hồi của bot
                var response = await _aiChatbotService.GetResponseAsync(msg);
                var botMessage = new ChatMessage
                {
                    Content = response,
                    IsFromUser = false,
                    Timestamp = DateTime.Now
                };
                ChatMessages.Add(botMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = new ChatMessage
                {
                    Content = $"Lỗi: {ex.Message}",
                    IsFromUser = false,
                    Timestamp = DateTime.Now
                };
                ChatMessages.Add(errorMessage);
            }
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

                    if (response.Pagination != null)
                    {
                        TotalCount = response.Pagination.TotalCount;
                        TotalPage = response.Pagination.TotalPage;
                        CurrentPage = response.Pagination.PageNumber;
                    }

                    hasMoreItems = response.Result.Count == DefaultPageSize && _currentPage < TotalPage;
                    if (hasMoreItems)
                    {
                        _currentPage++;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading stores: {ex.Message}");
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
            _currentPage = 1;
            hasMoreItems = true;
            await LoadStoresAsync();
        }

        public void ResetState()
        {
            _isLoaded = false;
            _currentPage = 1;
            hasMoreItems = true;
            stores.Clear();
            TotalCount = 0;
            TotalPage = 0;
            CurrentPage = 1;
            PaginationInfo = "";
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