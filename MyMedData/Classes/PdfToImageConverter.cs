using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
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

		
		public List<BitmapImage> Images { get; private set; }

		public async Task ReadPdfFromBytes(byte[] bytes)
		{
			using var ms = new InMemoryRandomAccessStream();
			await ms.WriteAsync(bytes.AsBuffer());
			ms.Seek(0);
			var doc = await PdfDocument.LoadFromStreamAsync(ms).AsTask();
			await PdfToImages(doc);
		}		

		private async Task PdfToImages(PdfDocument pdfDoc)
		{
			if (pdfDoc == null) return;

			Images.Clear();

			for (uint i = 0; i < pdfDoc.PageCount; i++)
			{
				using (var page = pdfDoc.GetPage(i))
				{
					var bitmap = await PageToBitmapAsync(page);					
					Images.Add(bitmap);
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
