using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.ViewModels
{
    public partial class AiChatbotViewModel : ObservableObject
    {
        private readonly IAiChatbotService _aiService;

        [ObservableProperty]
        private string inputMessage = string.Empty;

        [ObservableProperty]
        private string responseMessage = string.Empty;

        [ObservableProperty]
        private bool isBusy;

        public AiChatbotViewModel(IAiChatbotService aiService)
        {
            _aiService = aiService;
        }

        [RelayCommand]
        private async Task SendAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                var msg = InputMessage?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(msg)) return;
                var result = await _aiService.GetResponseAsync(msg);
                ResponseMessage = result;
            }
            catch (Exception ex)
            {
                ResponseMessage = $"Lỗi: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}


