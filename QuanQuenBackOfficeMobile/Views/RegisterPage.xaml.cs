namespace QuanQuenBackOfficeMobile.Views;

public partial class RegisterPage : ContentPage
{
	public RegisterPage()
	{
		InitializeComponent();
	}
    void OnBackButtonClicked(object sender, EventArgs e)
    {
        // go back to previous page
        Shell.Current.GoToAsync("..");
    }
}