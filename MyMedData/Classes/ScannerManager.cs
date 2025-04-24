using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using MyMedData.Windows;
using NAPS2;
using NAPS2.Wia;
using Windows.UI.Composition;
using static System.Math;


namespace MyMedData.Classes
{
	internal static class ScannerManager
	{
		public static string[] GetDeviceNames()
		{
			using var deviceManager = new WiaDeviceManager();
			return deviceManager.GetDeviceInfos().Select(info => info.Name()).ToArray();
		}
				
		public const int A4_INCH_WIDTH = 8_270;
		public const int A4_INCH_HEIGHT = 11_670;
		public const int A5_INCH_WIDTH = 5_800;
		public const int A5_INCH_HEIGHT = 8_300;
		public static string[] AVALIBLE_DPI => new string[] { "100", "150", "200", "300" };
		

		public static async Task<bool> CheckThatPrefereedScannerIsAvailible()
		{
			if (AppConfigDatabase.Settings.DefaultScannerName is not string preferredScannerName)
				return false;

			return await Task.Run(() => PingScanner(preferredScannerName));
		}

		private static Task<bool>? PingScanner(string scannerName)
		{
			using WiaDeviceManager wiaDeviceManager = new WiaDeviceManager();
			var devices = wiaDeviceManager.GetDeviceInfos();			
			return Task.FromResult(devices.Any(device => device.Name() == scannerName));
		}

		static (int X, int Y) GetPixelSize(PaperSize paperSize)
		{
			switch (paperSize) 
			{
				case PaperSize.A4:
					return new (A4_INCH_WIDTH, A4_INCH_HEIGHT); 					
				case PaperSize.A5:
					return new (A5_INCH_WIDTH, A5_INCH_HEIGHT);					
				default:
					throw new Exception("Не поддерживаемый формат бумаги");
			}
		}

		static TaskCompletionSource<BitmapSource?> scanTaskCompletion;
		static object scannerBusyLock = new object();
		static bool scannerBusy;
		public static bool ScannerIsBusy { get => scannerBusy; }

		public static async Task<BitmapSource> ScanAsync(PaperSize paperSize, int DPI_X, int DPI_Y)
		{
			if (!scannerBusy)
			{
				lock(scannerBusyLock)
				{
					if (!scannerBusy)
						scannerBusy = true;						
					else
						throw new ScannerBusyException();
				}
			}
			else
				throw new ScannerBusyException();

			ScanParamters scanParamters = new ScanParamters { PaperSize = paperSize, DPI_X = DPI_X, DPI_Y = DPI_Y };

			scanTaskCompletion = new TaskCompletionSource<BitmapSource>();
			await Task.Run(() => DoScan(scanParamters));
			return await scanTaskCompletion.Task;
		}

		private static async Task DoScan(object parameters)
		{
			var scanParamters = (ScanParamters)parameters;
			PaperSize paperSize = scanParamters.PaperSize;
			int DPI_X = scanParamters.DPI_X;
			int DPI_Y = scanParamters.DPI_Y;

			try
			{
				using WiaDeviceManager wiaDeviceManager = new WiaDeviceManager();
				var devices = wiaDeviceManager.GetDeviceInfos();
				var scannerName = AppConfigDatabase.Settings.DefaultScannerName;
				if (scannerName == null)
				{
					MessageBox.Show("Сканер не выбран. Выберите сканер в настройках приложения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
					scanTaskCompletion.TrySetResult(null);
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
					scanTaskCompletion.TrySetResult(null);
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

				var size = GetPixelSize(paperSize);
				scanner.SetProperty(WiaPropertyId.IPS_XRES, DPI_X);
				scanner.SetProperty(WiaPropertyId.IPS_YRES, DPI_Y);
				scanner.SetProperty(WiaPropertyId.IPS_XEXTENT, size.X * DPI_X / 1000);
				scanner.SetProperty(WiaPropertyId.IPS_YEXTENT, size.Y * DPI_Y / 1000);
				scanner.SetProperty(WiaPropertyId.DPS_HORIZONTAL_BED_SIZE, size.X);
				scanner.SetProperty(WiaPropertyId.DPS_VERTICAL_BED_SIZE, size.Y);


				using var transfer = scanner.StartTransfer();
				transfer.PageScanned += Transfer_PageScanned;
				transfer.Progress += Scan_Progress; 

				transfer.Download();
			}
			finally
			{
				scannerBusy = false;
			}
		}

		private static void Scan_Progress(object? sender, WiaTransfer.ProgressEventArgs e)
		{
			Progress?.Invoke(sender, e);
		}

		private static void Transfer_PageScanned(object? sender, WiaTransfer.PageScannedEventArgs e)
		{
			var image = new BitmapImage();
			try
			{
				using (e.Stream)
				{
					image.BeginInit();
					image.CacheOption = BitmapCacheOption.OnLoad;
					image.StreamSource = e.Stream;
					image.EndInit();

					image.Freeze();
					scanTaskCompletion.TrySetResult(image);					
				}
			}
			catch (Exception ex) 
			{
				scanTaskCompletion.TrySetException(ex);
			}
		}		

		public static event EventHandler<WiaTransfer.ProgressEventArgs> Progress;		
	}

	public enum PaperSize
	{
		A4, A5
	}

	struct ScanParamters
	{
		public PaperSize PaperSize;
		public int DPI_X;
		public int DPI_Y;
	}

	internal class ScannerBusyException: Exception
	{
		public ScannerBusyException(): base() { }
		public ScannerBusyException(string? message): base(message)	{ }
	}
}
