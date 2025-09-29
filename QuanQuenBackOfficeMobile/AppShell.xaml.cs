using QuanQuenBackOfficeMobile.Views;

namespace QuanQuenBackOfficeMobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("Main", typeof(MainPage));
		Routing.RegisterRoute("Login", typeof(LoginPage));
		Routing.RegisterRoute("AffiliateLogin", typeof(AffiliateLoginPage));
		Routing.RegisterRoute("AffiliateRegister", typeof(AffiliateRegisterPage));
    }
}
