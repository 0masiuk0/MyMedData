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
using static System.Math;

namespace MyMedData.Windows
{
	/// <summary>
	/// Interaction logic for ImageViewWindow.xaml
	/// </summary>
	public partial class ImageViewWindow : Window
	{
		public ImageViewWindow()
		{
			InitializeComponent();
		}		
		
		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
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
		}
	}
}
