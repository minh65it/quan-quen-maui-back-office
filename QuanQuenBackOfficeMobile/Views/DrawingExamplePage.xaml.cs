using CommunityToolkit.Maui.Views;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuanQuenBackOfficeMobile.Views;

public partial class DrawingExamplePage : ContentPage
{
	private bool _isBusy = false;

	public DrawingExamplePage()
	{
		InitializeComponent();
		var storeMediaService = App.Services.GetService<IStoreMediaService>();
		BindingContext = new DrawingExampleViewModel(storeMediaService!);
	}

	private async void SaveClicked(object sender, EventArgs e)
	{
		if (_isBusy) return;
		_isBusy = true;
		try 
		{
			System.Diagnostics.Debug.WriteLine("[DEBUG] SaveClicked: Starting signature save process");
			var vm = BindingContext as DrawingExampleViewModel;
			var drawingLines = vm?.Lines.ToList();

			if (drawingLines is null || drawingLines.Count < 1)
			{
				System.Diagnostics.Debug.WriteLine("[DEBUG] SaveClicked: No signature drawn");
				await DisplayAlert("Lỗi", "Vui lòng vẽ chữ ký của bạn", "OK");
				return;
			}

			var points = drawingLines.SelectMany(x => x.Points).ToList();

			// Render ra stream gốc
			var originalStream = await DrawingView.GetImageStream(
				drawingLines,
				new Size(points.Max(x => x.X) - points.Min(x => x.X), points.Max(x => x.Y) - points.Min(x => x.Y)),
				Colors.Gray);

			// Copy stream cho ImageSource
			var imageBuffer = new MemoryStream();
			await originalStream.CopyToAsync(imageBuffer);
			imageBuffer.Position = 0;
			originalStream.Position = 0;

			GeneratedImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBuffer.ToArray()));

			originalStream.Position = 0;
			System.Diagnostics.Debug.WriteLine("[DEBUG] SaveClicked: Uploading signature...");
			await vm.UploadImageAsync(originalStream);
			
			System.Diagnostics.Debug.WriteLine("[DEBUG] SaveClicked: Upload successful, navigating to StoreUserAuthentication");
			await Navigation.PushAsync(new QuanQuenBackOfficeMobile.Views.Stores.StoreUserAuthentication(StoreRegistrationData.Email));
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[DEBUG] SaveClicked error: {ex}");
			await DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
		}
		finally
		{
			_isBusy = false;
		}
	}
}
