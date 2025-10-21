using CommunityToolkit.Mvvm.ComponentModel;

namespace QuanQuenBackOfficeMobile.Models
{
    public partial class CreateFoodFormModel : ObservableObject
    {
        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private string price = string.Empty;

        [ObservableProperty]
        private string priceSale = string.Empty;

        public string StoreId { get; set; } = string.Empty;
        public string FilterFoodId { get; set; } = string.Empty;
        public string FilterName { get; set; } = string.Empty;
    }
}

