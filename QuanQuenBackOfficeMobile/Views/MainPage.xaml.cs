namespace QuanQuenBackOfficeMobile.Views;

public partial class MainPage : Shell
{
	public MainPage(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
