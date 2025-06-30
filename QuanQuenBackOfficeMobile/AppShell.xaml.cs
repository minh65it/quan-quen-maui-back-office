namespace QuanQuenBackOfficeMobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("Main", typeof(MainPage));

    }
}
