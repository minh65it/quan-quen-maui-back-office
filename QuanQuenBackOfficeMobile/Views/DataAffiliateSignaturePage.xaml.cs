using QuanQuenBackOfficeMobile.ViewModels;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using System.Web;
using CommunityToolkit.Maui.Core;

namespace QuanQuenBackOfficeMobile.Views;

public partial class DataAffiliateSignaturePage : ContentPage
{
    private DataAffiliateSignatureViewModel _viewModel;
    private bool _isSubscribed;

    public DataAffiliateSignaturePage(DataAffiliateSignatureViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (_viewModel != null)
        {
            // Ưu tiên lấy dữ liệu từ Preferences (được set khi tạo preview hoặc ở bước nhập info)
            var pdfBase64 = Preferences.Get("data_affiliate_preview_pdf_base64_for_sign", string.Empty);
            var storeName = Preferences.Get("data_affiliate_preview_store_name_for_sign", "Nhà hàng");

            // Thông tin đã lưu sẵn từ bước nhập Info
            var affiliateName = Preferences.Get("data_affiliate_signature_affiliate_name", string.Empty);
            var affiliatePhone = Preferences.Get("data_affiliate_signature_affiliate_phone", string.Empty);
            var representativeName = Preferences.Get("data_affiliate_signature_representative_name", string.Empty);
            var storePhone = Preferences.Get("data_affiliate_signature_store_phone", string.Empty);
            var storeId = Preferences.Get("data_affiliate_signature_store_id", string.Empty);
            var storeAddress = Preferences.Get("data_affiliate_signature_store_address", string.Empty);

            await _viewModel.InitializeAsync(pdfBase64, storeName, affiliateName, affiliatePhone, representativeName, storePhone, storeId, storeAddress);

            // Xóa để tránh dữ liệu cũ
            Preferences.Remove("data_affiliate_preview_pdf_base64_for_sign");
            Preferences.Remove("data_affiliate_preview_store_name_for_sign");
        }
    }

    protected override bool OnBackButtonPressed()
    {
        return base.OnBackButtonPressed();
    }

    // Đơn giản: Không xử lý xuất ảnh ở code-behind; ViewModel sẽ tự tạo bytes từ Lines khi bấm Hoàn tất
}
