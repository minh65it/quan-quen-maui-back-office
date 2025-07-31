using CommunityToolkit.Maui.Views;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

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
			var vm = BindingContext as DrawingExampleViewModel;
			var contractLines = vm?.ContractLines?.ToList();
			var appendixLines = vm?.AppendixLines?.ToList();

			if (contractLines == null || contractLines.Count < 1 || appendixLines == null || appendixLines.Count < 1)
			{
				await DisplayAlert("Lỗi", "Vui lòng vẽ đầy đủ cả hai chữ ký!", "OK");
				return;
			}

			// Render stream cho chữ ký phụ lục
			var appendixPoints = appendixLines.SelectMany(x => x.Points).ToList();
			var appendixStream = await DrawingView.GetImageStream(
				appendixLines,
				new Size(appendixPoints.Max(x => x.X) - appendixPoints.Min(x => x.X), appendixPoints.Max(x => x.Y) - appendixPoints.Min(x => x.Y)),
				Colors.Transparent
			);
			var appendixBuffer = new MemoryStream();
			await appendixStream.CopyToAsync(appendixBuffer);
			appendixBuffer.Position = 0;
			appendixStream.Position = 0;
			AppendixGeneratedImage.Source = ImageSource.FromStream(() => new MemoryStream(appendixBuffer.ToArray()));
			appendixStream.Position = 0;

			// Render stream cho chữ ký hợp đồng
			var contractPoints = contractLines.SelectMany(x => x.Points).ToList();
			var contractStream = await DrawingView.GetImageStream(
				contractLines,
				new Size(contractPoints.Max(x => x.X) - contractPoints.Min(x => x.X), contractPoints.Max(x => x.Y) - contractPoints.Min(x => x.Y)),
				Colors.Transparent
			);
			var contractBuffer = new MemoryStream();
			await contractStream.CopyToAsync(contractBuffer);
			contractBuffer.Position = 0;
			contractStream.Position = 0;
			ContractGeneratedImage.Source = ImageSource.FromStream(() => new MemoryStream(contractBuffer.ToArray()));
			contractStream.Position = 0;

			// Upload: phụ lục trước, hợp đồng sau
			await vm.UploadBothSignaturesAsync(appendixStream, contractStream);
			
			// Chuyển đến trang PhotoStoreUser sau khi lưu thành công
			await Application.Current.MainPage.Navigation.PushAsync(new PhotoStoreUser());
		}
		catch (Exception ex)
		{
			await DisplayAlert("Lỗi", ex.Message, "OK");
		}
		finally
		{
			_isBusy = false;
		}
	}
}
