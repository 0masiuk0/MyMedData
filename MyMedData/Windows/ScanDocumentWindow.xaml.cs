using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MyMedData.Classes;
using NAPS2.Wia;
using static System.Net.Mime.MediaTypeNames;
using static NAPS2.Wia.WiaPropertyId;

namespace MyMedData.Windows
{
	/// <summary>
	/// Interaction logic for ScanDocumentWindow.xaml
	/// </summary>
	public partial class ScanDocumentWindow : Window
	{
		public ScanDocumentWindow()
		{
			InitializeComponent();
		}

		private void ScanButton_Click(object sender, RoutedEventArgs e)
		{
			using WiaDeviceManager wiaDeviceManager = new WiaDeviceManager();
			var devices = wiaDeviceManager.GetDeviceInfos();
			var scannerName = ScannerManager.GetSetScannerName();
			if (scannerName == null)
			{
				MessageBox.Show("Сканер не выбран. Выберите сканер в настройках приложения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			WiaDevice scannerDevice;
			if (devices.FirstOrDefault(d => d.Name() == scannerName, null) is WiaDeviceInfo scannerInfo)
			{
				scannerDevice = wiaDeviceManager.FindDevice(scannerInfo.Id());
			}
			else 
			{
				MessageBox.Show("Выбранный сканер недоступен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			string scannerType;
			if (scannerDevice.SupportsFeeder())
			{
				scannerType = "Feeder";
			}
			else
			{
				scannerType = "Flatbed";
			}

			using WiaItem scanner = scannerDevice.FindSubItem(scannerType);
			if (scanner == null) throw new ScannerOperationException("Не удалось подключитсья к сканеру");

			scanner.SetProperty(WiaPropertyId.IPS_XRES, 150);
			scanner.SetProperty(WiaPropertyId.IPS_YRES, 150);

			using var transfer = scanner.StartTransfer();
			transfer.PageScanned += Transfer_PageScanned;
			

			// Do the actual scan
			transfer.Download();
		}

		private void Transfer_PageScanned(object? sender, WiaTransfer.PageScannedEventArgs e)
		{
			var image = new BitmapImage();
			using (e.Stream)
			{
				image.BeginInit();
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.StreamSource = e.Stream;
				image.EndInit();

				image.Freeze();
				images.Add(image);
				SetImage(images.Count - 1);
			}
		}



		#region stuff
		List<BitmapImage> images = new List<BitmapImage>();
		int? index = null;

		private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			images.Clear();
			if (e.NewValue is IEnumerable<BitmapImage> newImages && newImages.Count() > 0)
			{
				images.AddRange(newImages);
				index = 0;
				SetImage(0);
			}
			else
			{
				index = null;
				ClearImage();
			}
		}

		private void SetImage(int v)
		{
			if (images.Count == 0)
				return;
			ImageUI.Source = images[v];
			index = v;
		}

		private void ClearImage()
		{
			ImageUI.ClearValue(System.Windows.Controls.Image.SourceProperty);
		}

		private void PrevButton_Click(object sender, RoutedEventArgs e)
		{
			if (images.Count > 0 && index is int i)
			{
				if (i > 0)
					i--;
				else
					i = images.Count - 1;

				SetImage(i);
			}
			else
			{
				ClearImage();
			}
		}

		private void NextButton_Click(object sender, RoutedEventArgs e)
		{
			if (images.Count > 0 && index is int i)
			{
				if (i < images.Count - 1)
					i++;
				else
					i = 0;

				SetImage(i);
			}
			else
			{
				ClearImage();
			}
		}

		private void RestoreButton_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Normal;
			RestoreButton.Visibility = Visibility.Collapsed;
			MaximizeButton.Visibility = Visibility.Visible;
			Padding = new Thickness(4);
		}

		private void MaximizeButton_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Maximized;
			MaximizeButton.Visibility = Visibility.Collapsed;
			RestoreButton.Visibility = Visibility.Visible;
			Padding = new Thickness(12);
		}

		private void MinimizeButton_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}

		private void ResetButton_Click(object sender, RoutedEventArgs e)
		{
			ImageZoomBorder.Reset();
			SetImage(0);
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
		#endregion
	}

	internal class ScannerOperationException: Exception
	{
		public ScannerOperationException(string message) : base(message) { }
	}
}
