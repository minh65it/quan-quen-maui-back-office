namespace QuanQuenBackOfficeMobile.Views;

public partial class ExampleMaterialFontUsagePage : ContentPage
{
	public ExampleMaterialFontUsagePage(ExampleMaterialFontUsageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
