using Android.App;
using Android.Content.PM;
using Android.OS;

namespace QuanQuenBackOfficeMobile;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Set status bar color to orange (#FF5722)
        Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#B3F58220"));
    }
}
