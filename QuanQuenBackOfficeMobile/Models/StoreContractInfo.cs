namespace QuanQuenBackOfficeMobile.Models
{
    public class PersonalInfoDto
    {
        public string Name { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public string IdDate { get; set; } = string.Empty;
        public string IdPlace { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string PermanentAddress { get; set; } = string.Empty; 
        public string TemporaryAddress { get; set; } = string.Empty; 
    }

    public class ContractDto
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
        public string StoreHeadOfficeAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        
        // Thông tin đại diện và chủ hộ kinh doanh
        public PersonalInfoDto Representative { get; set; } = new PersonalInfoDto();
        public PersonalInfoDto Owner { get; set; } = new PersonalInfoDto();
    }


    public class MainContractDto : ContractDto
    {

        public string BankAccountNumber { get; set; } = string.Empty;
        public string BankAccountHolder { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
    }

    public class AppendixContractDto : ContractDto
    {

        public string AppendixSignDate { get; set; } = string.Empty; 
        public string AppendixNumber { get; set; } = string.Empty; 
    }

    /// <summary>
    /// DTO cho thông tin thỏa thuận
    /// </summary>
    public class AgreementInfoDto
    {
        public string StoreName { get; set; } = string.Empty;
        public string RepresentativeName { get; set; } = string.Empty;
        public string RepresentativePosition { get; set; } = string.Empty;
        public string RepresentativeIdNumber { get; set; } = string.Empty;
        public string StoreHeadOfficeAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO để gửi data đến endpoint PreviewCooperationAgreement
    /// </summary>
    public class PreviewAgreementRequest
    {
        public string StoreId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string SignatureFile { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string RepresentativeName { get; set; } = string.Empty;
        public string RepresentativePosition { get; set; } = string.Empty;
        public string RepresentativeIdNumber { get; set; } = string.Empty;
        public string StoreHeadOfficeAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response từ API PreviewCooperationAgreement
    /// </summary>
    public class PreviewAgreementResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string AgreementId { get; set; } = string.Empty;
        public string AgreementNumber { get; set; } = string.Empty;
        
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