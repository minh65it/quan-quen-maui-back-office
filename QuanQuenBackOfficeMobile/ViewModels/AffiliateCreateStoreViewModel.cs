using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Views;
using QuanQuenBackOfficeMobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace QuanQuenBackOfficeMobile.ViewModels;
    public partial class AffiliateCreateStoreViewModel : ObservableObject
    {
        private readonly IAffiliateService _affiliateService;
        private readonly GeographicApiService _geoService;
        private readonly CatergoryMasterService _catergoryMasterService;

        public ObservableCollection<ProvinceDto> ProvinceList { get; } = new();
        public ObservableCollection<DistrictDto> DistrictList { get; } = new();
        public ObservableCollection<WardDto> WardList { get; } = new();
        public ObservableCollection<string> BusinessTypeList { get; } = new();

        public ObservableCollection<CategoryMasterModel> CategoryMasterList { get; } = new();

        [ObservableProperty]
        private ProvinceDto selectedProvince;
        [ObservableProperty]
        private DistrictDto selectedDistrict;
        [ObservableProperty]
        private WardDto selectedWard;
        [ObservableProperty]
        private string selectedBusinessType;
        [ObservableProperty]
        private CategoryMasterModel selectCategotyMaster;

        [ObservableProperty]
        private string storeName;
        [ObservableProperty]
        private string representativeName;
        [ObservableProperty]
        private string address;
        [ObservableProperty]
        private string phoneNumber;
        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string phoneError = "";
        [ObservableProperty]
        private string emailError = "";
        [ObservableProperty]
        private string storeNameError = "";
        [ObservableProperty]
        private string representativeNameError = "";
        [ObservableProperty]
        private string addressError = "";
        [ObservableProperty]
        private string businessTypeError = "";
        [ObservableProperty]
        private string categoryMasterError = "";

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool hasMinimumTables = false;

        [ObservableProperty]
        private bool isFAndB = false;

        [ObservableProperty]
        private bool canLinkWithApps = false;

        public ICommand CreateStoreCommand { get; }

        public AffiliateCreateStoreViewModel(IAffiliateService affiliateService,
            GeographicApiService geoService, CatergoryMasterService catergoryMasterService)
        {
            _affiliateService = affiliateService;
            _geoService = geoService;
            _catergoryMasterService = catergoryMasterService;

            LoadProvincesAsync();
            LoadCategoryMaster();
            BusinessTypeList.Add("Tư Nhân");
            BusinessTypeList.Add("Công Ty");
            BusinessTypeList.Add("Hộ Kinh Doanh");
            
            CreateStoreCommand = new Command(async () => await CreateStoreAsync());
        }

        partial void OnSelectedProvinceChanged(ProvinceDto value)
        {
            LoadDistrictsAsync();
        }

        partial void OnSelectedDistrictChanged(DistrictDto value)
        {
            LoadWardsAsync();
        }

        private async void LoadCategoryMaster()
        {
            var categories = await _catergoryMasterService.GetCategoryMastersAsync();
            foreach (var c in categories)
            {
                CategoryMasterList.Add(c);
            }
        }

        private async Task LoadProvincesAsync()
        {
            try
            {
                var provinces = await _geoService.GetProvincesAsync();
                ProvinceList.Clear();
                foreach (var province in provinces)
                {
                    ProvinceList.Add(province);
                }
            }
            catch (Exception ex)
            {
                // Handle error
            }
        }

        private async Task LoadDistrictsAsync()
        {
            if (SelectedProvince == null) return;

            try
            {
                var districts = await _geoService.GetDistrictsAsync(SelectedProvince.zoneId);
                DistrictList.Clear();
                foreach (var district in districts)
                {
                    DistrictList.Add(district);
                }
            }
            catch (Exception ex)
            {
                // Handle error
            }
        }

        private async Task LoadWardsAsync()
        {
            if (SelectedDistrict == null) return;

            try
            {
                var wards = await _geoService.GetWardsAsync(SelectedDistrict.territoryId);
                WardList.Clear();
                foreach (var ward in wards)
                {
                    WardList.Add(ward);
                }
            }
            catch (Exception ex)
            {
                // Handle error
            }
        }



        private async Task CreateStoreAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ClearAllErrors();

                // Validate all fields
                bool hasError = false;

                if (string.IsNullOrWhiteSpace(StoreName))
                {
                    StoreNameError = "Vui lòng nhập tên nhà hàng";
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(RepresentativeName))
                {
                    RepresentativeNameError = "Vui lòng nhập tên người đại diện";
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(Address))
                {
                    AddressError = "Vui lòng nhập địa chỉ";
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(PhoneNumber))
                {
                    PhoneError = "Vui lòng nhập số điện thoại";
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(Email))
                {
                    EmailError = "Vui lòng nhập email";
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(SelectedBusinessType))
                {
                    BusinessTypeError = "Vui lòng chọn loại hình kinh doanh";
                    hasError = true;
                }
                if (SelectCategotyMaster == null)
                {
                    CategoryMasterError = "Vui lòng chọn thể loại cửa hàng";
                    hasError = true;
                }

                if (hasError)
                {
                    IsBusy = false;
                    return;
                }

                // Save data to static class for next page
                AffiliateStoreData.StoreName = StoreName;
                AffiliateStoreData.RepresentativeName = RepresentativeName;
                AffiliateStoreData.Address = Address;
                AffiliateStoreData.PhoneNumber = PhoneNumber;
                AffiliateStoreData.Email = Email;
                AffiliateStoreData.ProvinceId = SelectedProvince?.zoneId ?? "";
                AffiliateStoreData.DistrictId = SelectedDistrict?.territoryId ?? "";
                AffiliateStoreData.WardId = SelectedWard?.stateId ?? "";
                AffiliateStoreData.SelectedBusinessType = SelectedBusinessType;
                AffiliateStoreData.StoreTypeMasterId = SelectCategotyMaster?.Id ?? "";

                // Cũng lưu vào StoreRegistrationData để dùng cho agreement (giống user thường)
                StoreRegistrationData.StoreName = StoreName;
                StoreRegistrationData.RepresentativeName = RepresentativeName;
                StoreRegistrationData.Address = Address;
                StoreRegistrationData.PhoneNumber = PhoneNumber;
                StoreRegistrationData.Email = Email;
                StoreRegistrationData.Province = SelectedProvince?.Name ?? "";
                StoreRegistrationData.District = SelectedDistrict?.Name ?? "";
                StoreRegistrationData.Ward = SelectedWard?.Name ?? "";
                StoreRegistrationData.ProvinceId = SelectedProvince?.zoneId ?? "";
                StoreRegistrationData.DistrictId = SelectedDistrict?.territoryId ?? "";
                StoreRegistrationData.WardId = SelectedWard?.stateId ?? "";
                StoreRegistrationData.BusinessType = SelectedBusinessType;
                StoreRegistrationData.CatergoryMaster = SelectCategotyMaster?.Id ?? "";

                // Get referrer ID from affiliate login data
                try
                {
                    AffiliateStoreData.ReferrerId = await SecureStorage.GetAsync("affiliate_referral_id") ?? "";
                }
                catch
                {
                    AffiliateStoreData.ReferrerId = Preferences.Get("affiliate_referral_id", "");
                }

                // Navigate to password page
                var affiliateStoreUserPasswordViewModel = IPlatformApplication.Current.Services.GetService<AffiliateStoreUserPasswordViewModel>();
                Application.Current.MainPage = new AffiliateStoreUserPasswordPage(affiliateStoreUserPasswordViewModel);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ClearAllErrors()
        {
            StoreNameError = "";
            RepresentativeNameError = "";
            AddressError = "";
            PhoneError = "";
            EmailError = "";
            BusinessTypeError = "";
        }
    }

