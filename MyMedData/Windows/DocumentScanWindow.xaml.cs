using System;
using System.Collections.Generic;
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

namespace MyMedData.Windows
{
	/// <summary>
	/// Interaction logic for DocumentScanWindow.xaml
	/// </summary>
	public partial class DocumentScanWindow : Window
	{
		public DocumentScanWindow()
		{
			InitializeComponent();
		}

		private void ScanButton_Click(object sender, RoutedEventArgs e)
		{
			Scan();
		}

		private async void Scan()
		{
			Scanner scanner = new Scanner(this);
			scanner.StartScan();
			scanner.
		}

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
			ImageUI.ClearValue(Image.SourceProperty);
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

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}		
	}
}
