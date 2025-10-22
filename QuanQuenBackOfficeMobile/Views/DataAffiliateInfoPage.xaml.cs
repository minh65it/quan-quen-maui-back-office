using QuanQuenBackOfficeMobile.ViewModels;

namespace QuanQuenBackOfficeMobile.Views;

public partial class DataAffiliateInfoPage : ContentPage
{
    public DataAffiliateInfoPage(DataAffiliateInfoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (BindingContext is DataAffiliateInfoViewModel vm)
        {
            var route = Shell.Current?.CurrentState.Location.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(route)) return;

            try
            {
                var uri = new Uri(route);
                var query = uri.Query;
                var parameters = System.Web.HttpUtility.ParseQueryString(query);
                var storeId = parameters.Get("storeId");
                var storeName = parameters.Get("storeName");
                var address = parameters.Get("address");
                vm.InitializeFromParams(storeId, storeName, address);
            }
            catch { }
        }
    }
}


