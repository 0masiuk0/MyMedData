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

		private async void ScanButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				ScanButton.IsEnabled = false;
				ScannedImage = await ScannerManager.ScanAsync(PaperSize.A4);
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
				ScanButton.IsEnabled = true;
			}
		}

		#region stuff
		public BitmapImage? ScannedImage 
		{
			get => (BitmapImage)GetValue(ScannedImageProperty);
			private set => SetValue(ScannedImageProperty, value); 
		}

		public static DependencyProperty ScannedImageProperty =
			DependencyProperty.Register(nameof(ScannedImage), typeof(BitmapImage), typeof(ScanDocumentWindow), new PropertyMetadata());

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
	}

	internal class ScannerOperationException: Exception
	{
		public ScannerOperationException(string message) : base(message) { }
	}
}
