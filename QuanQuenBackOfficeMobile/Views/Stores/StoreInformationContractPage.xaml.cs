namespace QuanQuenBackOfficeMobile.Views.Stores;

using QuanQuenBackOfficeMobile.ViewModels.Stores;

public partial class StoreInformationContractPage : ContentPage
{
	public StoreInformationContractPage(StoreInformationContractViewModel viewModel)
	{
		InitializeComponent();
		this.BindingContext = viewModel;
	}
}