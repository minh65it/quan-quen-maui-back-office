namespace QuanQuenBackOfficeMobile.Views;

public partial class MapPage : ContentPage
{
	public MapPage(MapViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
