using CommunityToolkit.Maui.Views;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.ViewModels.Stores;
using System;
using System.IO;
using System.Linq;
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
	private StoreInformationContractViewModel? _contractData;

	public DrawingExamplePage()
	{
		InitializeComponent();
		var storeMediaService = App.Services.GetService<IStoreMediaService>();
		BindingContext = new DrawingExampleViewModel(storeMediaService!);
	}

	// Constructor với contract data
	public DrawingExamplePage(StoreInformationContractViewModel contractData)
	{
		InitializeComponent();
		var storeMediaService = App.Services.GetService<IStoreMediaService>();
		BindingContext = new DrawingExampleViewModel(storeMediaService!);
		_contractData = contractData;
	}

	private async void SaveClicked(object sender, EventArgs e)
	{
		if (_isBusy) return;
		_isBusy = true;
		try 
		{
			var vm = BindingContext as DrawingExampleViewModel;
			var contractLines = vm?.ContractLines;
			var appendixLines = vm?.AppendixLines;

			if (contractLines == null || contractLines.Count < 1 || appendixLines == null || appendixLines.Count < 1)
			{
				await DisplayAlert("Lỗi", "Vui lòng vẽ đầy đủ cả hai chữ ký!", "OK");
				return;
			}

			// Kiểm tra có contract data không
			if (_contractData == null)
			{
				await DisplayAlert("Lỗi", "Không tìm thấy thông tin hợp đồng!", "OK");
				return;
			}

			// Generate signature images
			var contractSignatureBytes = await GenerateSignatureImage(ContractDrawingView);
			var appendixSignatureBytes = await GenerateSignatureImage(AppendixDrawingView);

			if (contractSignatureBytes == null || appendixSignatureBytes == null)
			{
				await DisplayAlert("Lỗi", "Không thể tạo ảnh chữ ký!", "OK");
				return;
			}

			// Navigate to ContractStorePage với signatures
			var contractStorePage = new QuanQuenBackOfficeMobile.Views.Stores.ContractStorePage(
				_contractData, 
				contractLines, 
				appendixLines,
				contractSignatureBytes,
				appendixSignatureBytes
			);

			await Navigation.PushAsync(contractStorePage);
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

	private async Task<byte[]?> GenerateSignatureImage(CommunityToolkit.Maui.Views.DrawingView drawingView)
	{
		try
		{
			// Get all points from drawing lines
			var points = drawingView.Lines.SelectMany(x => x.Points).ToList();
			if (!points.Any())
			{
				System.Diagnostics.Debug.WriteLine("[DrawingExamplePage] No points found for signature generation");
				return null;
			}

			// Calculate dynamic size based on drawing bounds
			var dynamicSize = new Microsoft.Maui.Graphics.Size(
				points.Max(x => x.X) - points.Min(x => x.X), 
				points.Max(x => x.Y) - points.Min(x => x.Y)
			);

			// Render stream with transparent background
			var imageStream = await CommunityToolkit.Maui.Views.DrawingView.GetImageStream(
				drawingView.Lines,
				dynamicSize,
				Colors.Transparent
			);

			if (imageStream != null)
			{
				using var memoryBuffer = new MemoryStream();
				await imageStream.CopyToAsync(memoryBuffer);
				memoryBuffer.Position = 0;
				
				// Return as byte array
				return memoryBuffer.ToArray();
			}
			return null;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[DrawingExamplePage] Error generating signature image: {ex.Message}");
			return null;
		}
	}
}
