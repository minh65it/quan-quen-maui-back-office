using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Storage;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.ViewModels;

public partial class DrawingExampleViewModel : BaseViewModel
{
    private readonly IStoreMediaService _storeMediaService;
    [ObservableProperty]
	public ObservableCollection<IDrawingLine> lines = new();

	private string _signatureCategoryMediaId;
    public DrawingExampleViewModel(IStoreMediaService storeMediaService)
    {
        _storeMediaService = storeMediaService;
    }

    [RelayCommand]
	public void Clear()
	{
		Lines?.Clear();
	}

    public async Task UploadImageAsync(Stream imageStream)
    {
        try
        {
            if (imageStream.CanSeek)
                imageStream.Position = 0;
            var storeId = StoreRegistrationData.StoreId;
            var createdBy = await SecureStorage.GetAsync("userName");
            var nameMedia = "Chữ kí"; 
            var categoryMediaId = await _storeMediaService.GetThumbnailCategoryMediaIdAsync(nameMedia);

            if (string.IsNullOrEmpty(storeId) || string.IsNullOrEmpty(createdBy) || string.IsNullOrEmpty(categoryMediaId))
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Thiếu thông tin upload!", "OK");
                return;
            }

            // Đọc stream thành byte[]
            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                await imageStream.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }

            var result = await _storeMediaService.UploadMediasAsync(
                storeId,
                categoryMediaId,
                createdBy!,
                new List<byte[]> { imageBytes },
                true,
                false
            );

            if (result.IsSuccess)
            {
                await App.Current.MainPage.DisplayAlert("Thành công", "Lưu chữ ký thành công!", "OK");
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", result.Message ?? "Upload ảnh thất bại", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] UploadImageAsync: Exception: {ex}");
            await App.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi khi upload ảnh: {ex.Message}", "OK");
        }
    }


}
