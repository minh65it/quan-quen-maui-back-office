namespace QuanQuenBackOfficeMobile.Models
{
    public class StoreContractInfo
    {
        public StoreMainContractModel MainContract { get; set; } = new StoreMainContractModel();
        public StoreAppendixContractModel AppendixContract { get; set; } = new StoreAppendixContractModel();
    }

    public class StoreMainContractModel
    {
        public string BusinessType { get; set; } // Loại hình kinh doanh: Tổ chức/Hộ kinh doanh/Tư Nhân
        public string CompanyName { get; set; }
        public string StoreName { get; set; }
        public string BusinessLicenseNumber { get; set; }
        public string BusinessLicenseDate { get; set; }
        public string BusinessLicensePlace { get; set; }
        public string RepresentativeName { get; set; }
        public string RepresentativePosition { get; set; }
        public string RepresentativeIdNumber { get; set; }
        public string RepresentativeIdDate { get; set; }
        public string RepresentativeIdPlace { get; set; }
        public string RepresentativePermanentAddress { get; set; }
        public string RepresentativeTemporaryAddress { get; set; }
        public string StoreHeadOfficeAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountHolder { get; set; }
        public string BankName { get; set; }
        // Hộ kinh doanh
        public string OwnerName { get; set; }
        public string OwnerGender { get; set; }
        public string OwnerIdNumber { get; set; }
        public string OwnerIdDate { get; set; }
        public string OwnerIdPlace { get; set; }
        public string OwnerPermanentAddress { get; set; }
        public string OwnerTemporaryAddress { get; set; }
    }

    public class StoreAppendixContractModel
    {
        public string BusinessType { get; set; } // Loại hình kinh doanh: Tổ chức/Hộ kinh doanh/Tư Nhân
        public string AppendixSignDate { get; set; }
        public string AppendixNumber { get; set; }
        public string CompanyName { get; set; }
        public string StoreName { get; set; }
        public string BusinessLicenseNumber { get; set; }
        public string BusinessLicenseDate { get; set; }
        public string BusinessLicensePlace { get; set; }
        public string RepresentativeName { get; set; }
        public string RepresentativePosition { get; set; }
        public string RepresentativeIdNumber { get; set; }
        public string RepresentativeIdDate { get; set; }
        public string RepresentativeIdPlace { get; set; }
        public string RepresentativePermanentAddress { get; set; }
        public string RepresentativeTemporaryAddress { get; set; }
        public string StoreHeadOfficeAddress { get; set; }
        public string PhoneNumber { get; set; }
        // Hộ kinh doanh
        public string OwnerName { get; set; }
        public string OwnerGender { get; set; }
        public string OwnerIdNumber { get; set; }
        public string OwnerIdDate { get; set; }
        public string OwnerIdPlace { get; set; }
        public string OwnerPermanentAddress { get; set; }
        public string OwnerTemporaryAddress { get; set; }
    }

    public class ContractData
    {
        public StoreMainContractModel MainContract { get; set; } = new StoreMainContractModel();
        public StoreAppendixContractModel AppendixContract { get; set; } = new StoreAppendixContractModel();
        public DateTime GeneratedDate { get; set; } = DateTime.Now;
        public string ContractNumber { get; set; } = $"CT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        
        // Thông tin bổ sung cho template HTML
        public string ContractLocation { get; set; } = "Thành phố Hồ Chí Minh";
        public string ServiceProviderIdNumber { get; set; } = "123456789";
        public string ServiceProviderIdDate { get; set; } = "01/01/2020";
        public string ServiceProviderIdPlace { get; set; } = "Công an TP.HCM";
    }

    // ========== API DTO FOR PREVIEW CONTRACT ==========

    /// <summary>
    /// DTO để gửi data đến endpoint PreviewContract
    /// </summary>
    public class PreviewContractRequest
    {
        public string StoreId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public MainContractDto MainContract { get; set; } = new MainContractDto();
        public AppendixContractDto AppendixContract { get; set; } = new AppendixContractDto();
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public object? File { get; set; }
        public object? File2 { get; set; }
    }

    /// <summary>
    /// MainContract DTO cho API
    /// </summary>
    public class MainContractDto
    {
        // Thông tin Bên Cung Cấp Dịch Vụ
        public string ProviderRepresentativeIdNumber { get; set; } = string.Empty;
        public string ProviderRepresentativeIdDate { get; set; } = string.Empty;
        public string ProviderRepresentativeIdPlace { get; set; } = string.Empty;
        
        // Thông tin Bên Thuê Dịch Vụ
        public string BusinessType { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string HeadquartersAddress { get; set; } = string.Empty;
        public string BusinessLicenseNumber { get; set; } = string.Empty;
        public string BusinessLicenseDate { get; set; } = string.Empty;
        public string BusinessLicensePlace { get; set; } = string.Empty;
        public string RepresentativeName { get; set; } = string.Empty;
        public string RepresentativePosition { get; set; } = string.Empty;
        public string RepresentativeIdNumber { get; set; } = string.Empty;
        public string RepresentativeIdDate { get; set; } = string.Empty;
        public string RepresentativeIdPlace { get; set; } = string.Empty;
        public string RepresentativePermanentAddress { get; set; } = string.Empty;
        public string RepresentativeTemporaryAddress { get; set; } = string.Empty;
        public string StoreHeadOfficeAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string BankAccountHolder { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        
        // Thông tin chủ hộ kinh doanh
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerGender { get; set; } = string.Empty;
        public string OwnerIdNumber { get; set; } = string.Empty;
        public string OwnerIdDate { get; set; } = string.Empty;
        public string OwnerIdPlace { get; set; } = string.Empty;
        public string OwnerPermanentAddress { get; set; } = string.Empty;
        public string OwnerTemporaryAddress { get; set; } = string.Empty;
    }

    /// <summary>
    /// AppendixContract DTO cho API
    /// </summary>
    public class AppendixContractDto
    {
        // Thông tin Bên Cung Cấp Dịch Vụ
        public string ProviderRepresentativeIdNumber { get; set; } = string.Empty;
        public string ProviderRepresentativeIdDate { get; set; } = string.Empty;
        public string ProviderRepresentativeIdPlace { get; set; } = string.Empty;
        
        // Thông tin Bên Thuê Dịch Vụ
        public string BusinessType { get; set; } = string.Empty;
        public string AppendixSignDate { get; set; } = string.Empty; // Backend sẽ tự tạo
        public string AppendixNumber { get; set; } = string.Empty; // Backend sẽ tự tạo
        public string CompanyName { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string HeadquartersAddress { get; set; } = string.Empty; // Consistent với MainContract
        public string BusinessLicenseNumber { get; set; } = string.Empty;
        public string BusinessLicenseDate { get; set; } = string.Empty;
        public string BusinessLicensePlace { get; set; } = string.Empty;
        public string RepresentativeName { get; set; } = string.Empty;
        public string RepresentativePosition { get; set; } = string.Empty;
        public string RepresentativeIdNumber { get; set; } = string.Empty;
        public string RepresentativeIdDate { get; set; } = string.Empty;
        public string RepresentativeIdPlace { get; set; } = string.Empty;
        public string RepresentativePermanentAddress { get; set; } = string.Empty;
        public string RepresentativeTemporaryAddress { get; set; } = string.Empty;
        public string StoreHeadOfficeAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string BankAccountHolder { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        
        // Thông tin chủ hộ kinh doanh
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerGender { get; set; } = string.Empty;
        public string OwnerIdNumber { get; set; } = string.Empty;
        public string OwnerIdDate { get; set; } = string.Empty;
        public string OwnerIdPlace { get; set; } = string.Empty;
        public string OwnerPermanentAddress { get; set; } = string.Empty;
        public string OwnerTemporaryAddress { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response từ API PreviewContract
    /// </summary>
    public class PreviewContractResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty; // Base64 PDF content
        public string ContractId { get; set; } = string.Empty;
        public string ContractNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Tạo data URL để hiển thị PDF trong WebView
        /// </summary>
        public string GetPdfDataUrl()
        {
            try
            {
                if (!string.IsNullOrEmpty(Message))
                {
                    return $"data:application/pdf;base64,{Message}";
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }

} 