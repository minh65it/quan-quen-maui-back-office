using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanQuenBackOfficeMobile.Services.Interfaces;

public interface IStoreMediaService
{
    /// <summary>
    /// Lấy categoryMediaId cho thumbnail
    /// </summary>
    Task<string> GetThumbnailCategoryMediaIdAsync(string nameMedia);

    /// <summary>
    /// Upload ảnh cho cửa hàng
    /// </summary>
    Task<ApiResponse> UploadMediasAsync(string storeId, string categoryMediaId, string createdBy, List<byte[]> imageBytesList, bool isSecurity = false, bool isMenu = false);

    /// <summary>
    /// Upload ảnh hóa đơn cho cửa hàng
    /// </summary>
    Task<ApiResponse> UploadInvoiceImageAsync(string storeId, int month, byte[] imageBytes, string fileName);
} 