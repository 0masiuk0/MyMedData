using MyMedData.Classes;
using MyMedData.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Math;


namespace MyMedData.Controls
{
	/// <summary>
	/// Логика взаимодействия для DocumentPlaque.xaml
	/// </summary>
	public partial class DocumentPlaque : UserControl
	{
		public DocumentPlaque()
		{
			InitializeComponent();
		}

		bool docIsOpen = false;

		private async Task ViewDocumentAttachment(AttachmentMetaData document)
		{
			try
			{
				docIsOpen = true;
				Session session = FindSession();
				var imageBytes = await document.LoadData(session);

				if (document.DocumentType == DocumentType.JPEG || document.DocumentType == DocumentType.PNG)
				{
					BitmapImage image = new BitmapImage();
					image.BeginInit();
					image.StreamSource = new MemoryStream(imageBytes);
					image.EndInit();

					ImageViewWindow imageViewWindow = new ImageViewWindow();
					imageViewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;					
					var width = Min(SystemParameters.WorkArea.Width, 1200);
					width = Max(SystemParameters.WorkArea.Width / 3, width);
					var height = Min(SystemParameters.WorkArea.Height, 800);
					height = Max(SystemParameters.WorkArea.Height / 3, height);
					imageViewWindow.Height = height;
					imageViewWindow.Width = width;
					imageViewWindow.DataContext = image;
					imageViewWindow.ShowDialog();
				}
				else if (document.DocumentType == DocumentType.PDF)
				{
					PdfToImageConverter converter = new PdfToImageConverter();
					await converter.ReadPdfFromBytes(imageBytes);

					PdfViewWindow pdfViewWindow = new PdfViewWindow();
					pdfViewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
					var width = Min(SystemParameters.WorkArea.Width, 1200);
					width = Max(SystemParameters.WorkArea.Width / 3, width);
					var height = Min(SystemParameters.WorkArea.Height, 800);
					height = Max(SystemParameters.WorkArea.Height / 3, height);
					pdfViewWindow.Height = height;
					pdfViewWindow.Width = width;
					pdfViewWindow.DataContext = converter.Images.ToList();
					pdfViewWindow.ShowDialog();
				}
			}
			catch(Exception ex)
			{
				MessageBox.Show("Непредвиденная ошибка", ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				docIsOpen = false;
			}
		}

		private async void Image_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed && !docIsOpen && DataContext is AttachmentMetaData atatchment)
				await ViewDocumentAttachment(atatchment);
		}

		private async void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (!docIsOpen && DataContext is AttachmentMetaData atatchment)
				await ViewDocumentAttachment(atatchment);
		}

		private Session FindSession()
		{
			var rightParent = FindParent(this);
			return ((FrameworkElement)rightParent).DataContext as Session;

			DependencyObject FindParent(DependencyObject child)
			{
				DependencyObject parent = VisualTreeHelper.GetParent(child);

				if (parent is FrameworkElement p
					&& p.DataContext is Session session)
					return parent;
				else
					return FindParent(parent);

			}
		}		
	}
}
