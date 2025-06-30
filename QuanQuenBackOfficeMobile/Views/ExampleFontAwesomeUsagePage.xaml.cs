namespace QuanQuenBackOfficeMobile.Views;

public partial class ExampleFontAwesomeUsagePage : ContentPage
{
	public ExampleFontAwesomeUsagePage(ExampleFontAwesomeUsageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
