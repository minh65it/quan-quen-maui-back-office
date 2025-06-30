namespace QuanQuenBackOfficeMobile.ViewModels;

[QueryProperty(nameof(Item), "Item")]
public partial class SettingsDetailViewModel : BaseViewModel
{
	[ObservableProperty]
	SampleItem? item;
}
