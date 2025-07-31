using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Storage;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Models;

namespace QuanQuenBackOfficeMobile.ViewModels;

public partial class DrawingExampleViewModel : BaseViewModel
{
    private readonly IStoreMediaService _storeMediaService;
    [ObservableProperty]
    public ObservableCollection<IDrawingLine> contractLines = new();
    [ObservableProperty]
    public ObservableCollection<IDrawingLine> appendixLines = new();

    public DrawingExampleViewModel(IStoreMediaService storeMediaService)
    {
        _storeMediaService = storeMediaService;
    }

    [RelayCommand]
    public void ClearContract()
    {
        ContractLines?.Clear();
    }

    [RelayCommand]
    public void ClearAppendix()
    {
        AppendixLines?.Clear();
    }

    public async Task UploadBothSignaturesAsync(Stream appendixStream, Stream contractStream)
    {
        try
        {
            if (appendixStream.CanSeek) appendixStream.Position = 0;
            if (contractStream.CanSeek) contractStream.Position = 0;
            var storeId = StoreRegistrationData.StoreId ?? "";
            var createdBy = await SecureStorage.GetAsync("userName");
            var nameMedia = "Chữ kí";
            var categoryMediaId = await _storeMediaService.GetThumbnailCategoryMediaIdAsync(nameMedia);

            if (string.IsNullOrEmpty(storeId) || string.IsNullOrEmpty(createdBy) || string.IsNullOrEmpty(categoryMediaId))
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Thiếu thông tin upload!", "OK");
                return;
            }

            byte[] appendixBytes, contractBytes;
            using (var ms = new MemoryStream()) { await appendixStream.CopyToAsync(ms); appendixBytes = ms.ToArray(); }
            using (var ms = new MemoryStream()) { await contractStream.CopyToAsync(ms); contractBytes = ms.ToArray(); }

            var result = await _storeMediaService.UploadMediasAsync(
                storeId,
                categoryMediaId,
                createdBy!,
                new List<byte[]> { appendixBytes, contractBytes },
                true,
                false
            );

            if (result.IsSuccess)
            {
                await App.Current.MainPage.DisplayAlert("Thành công", "Lưu hai chữ ký thành công!", "OK");
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", result.Message ?? "Upload ảnh thất bại", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] UploadBothSignaturesAsync: Exception: {ex}");
            await App.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi khi upload ảnh: {ex.Message}", "OK");
        }
    }
}
