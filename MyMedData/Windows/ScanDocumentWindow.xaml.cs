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
			LinearGradientBrush scanButtonBrush = (LinearGradientBrush)TryFindResource("scanButonGradientBrush");
			scanProgressGradientStop1 = scanButtonBrush.GradientStops[1];
			scanProgressGradientStop2 = scanButtonBrush.GradientStops[2];

			ScannerManager.AVALIBLE_DPI.ToList().ForEach(dpi => DPI_ComboBox.Items.Add(dpi));
			if (AppConfigDatabase.Settings.DPI is string dpi)
				DPI_ComboBox.SelectedItem = dpi;
			else
				DPI_ComboBox.SelectedIndex = 0;

			FormatComboBox.Items.Add("A4");
			FormatComboBox.Items.Add("A5");
			FormatComboBox.SelectedIndex = 0;
		}

		GradientStop scanProgressGradientStop1;
		GradientStop scanProgressGradientStop2;

		private async void ScanButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				ScanButton.IsEnabled = false;
				ScannerManager.Progress += ScannerManager_Progress;
				PaperSize paperSize = FormatComboBox.SelectedItem == "A5" ? PaperSize.A5 : PaperSize.A4;

				int DPI_X, DPI_Y;
				if (int.TryParse(DPI_ComboBox.SelectedItem?.ToString(), out DPI_X))
					DPI_Y = DPI_X;
				else
					DPI_X = DPI_Y = 100;

				ScannedImage = await ScannerManager.ScanAsync(paperSize, DPI_X, DPI_Y);
			}
			catch(ScannerBusyException)
			{
				MessageBox.Show("Сканер занят.", "Подождите", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message, "Неудача", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				ScannerManager.Progress -= ScannerManager_Progress;
				scanProgressGradientStop1.Offset = scanProgressGradientStop2.Offset = 1;
				ScanButton.IsEnabled = true;
			}
		}

		private void ScannerManager_Progress(object? sender, WiaTransfer.ProgressEventArgs e)
		{
			this.Dispatcher.BeginInvoke(
				() => scanProgressGradientStop1.Offset = scanProgressGradientStop2.Offset = 1 - ((double)e.Percent / 100));
		}

		private void Rotate(double angle)
		{
			TransformedBitmap transformedBitmap = new TransformedBitmap();
			transformedBitmap.BeginInit();
			transformedBitmap.Source = ScannedImage;
			transformedBitmap.Transform = new RotateTransform(angle, 0.5, 0.5);
			transformedBitmap.EndInit();
			transformedBitmap.Freeze();
			
			ScannedImage = transformedBitmap;
		}

		#region stuff
		public BitmapSource? ScannedImage 
		{
			get => (BitmapSource)GetValue(ScannedImageProperty);
			private set => SetValue(ScannedImageProperty, value); 
		}

		public static DependencyProperty ScannedImageProperty =
			DependencyProperty.Register(nameof(ScannedImage), typeof(BitmapSource), typeof(ScanDocumentWindow), new PropertyMetadata(ScannedImageChanged));

		private static void ScannedImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScanDocumentWindow window = (ScanDocumentWindow)d;
			if (e.NewValue is BitmapSource image)
				window.AcceptButtonsPanel.Visibility = Visibility.Visible;
			else 
				window.AcceptButtonsPanel.Visibility= Visibility.Collapsed;
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

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
		#endregion

		private void RotateLeftButton_Click(object sender, RoutedEventArgs e)
		{
			Rotate(-90.0);
		}

		private void RotateRightButton_Click(object sender, RoutedEventArgs e)
		{
			Rotate(90.0);
		}

		private void AcceptImageButton_Click(object sender, RoutedEventArgs e)
		{
			if (ScannedImage is BitmapSource image)
				Close();
		}

		private void DiscardImageButton_Click(object sender, RoutedEventArgs e)
		{
			ScannedImage = null;
		}
	}

	internal class ScannerOperationException: Exception
	{
		public ScannerOperationException(string message) : base(message) { }
	}
}
