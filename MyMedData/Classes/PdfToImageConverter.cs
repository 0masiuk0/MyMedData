using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MyMedData.Classes
{
	internal class PdfToImageConverter
	{
		public PdfToImageConverter() { Images = new(); }

		byte[] _pdfBytes;
		public byte[] PdfBytes
		{
			get => _pdfBytes;
			set
			{
				_pdfBytes = value;
				OnBytesChanged(value);
			}
		}

		public List<Image> Images { get; private set; }

		private async void OnBytesChanged(byte[] bytes)
		{
			using (var stream = new InMemoryRandomAccessStream())
			{
				using (var dataReader = new DataReader(stream))
				{
					dataReader.ReadBytes(bytes);
					var doc = await PdfDocument.LoadFromStreamAsync(stream).AsTask();
					await PdfToImages(doc);
				}
			}
		}

		private async Task PdfToImages( PdfDocument pdfDoc)
		{
			if (pdfDoc == null) return;

			Images.Clear();

			for (uint i = 0; i < pdfDoc.PageCount; i++)
			{
				using (var page = pdfDoc.GetPage(i))
				{
					var bitmap = await PageToBitmapAsync(page);
					var image = new Image
					{
						Source = bitmap,
						HorizontalAlignment = HorizontalAlignment.Center,
						Margin = new Thickness(0, 4, 0, 4),
						MaxWidth = 800
					};
					Images.Add(image);
				}
			}
		}

		private static async Task<BitmapImage> PageToBitmapAsync(PdfPage page)
		{
			BitmapImage image = new BitmapImage();

			using (var stream = new InMemoryRandomAccessStream())
			{
				await page.RenderToStreamAsync(stream);

				image.BeginInit();
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.StreamSource = stream.AsStream();
				image.EndInit();
			}

			return image;
		}
	}
}
