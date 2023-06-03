using MyMedData.Classes;
using MyMedData.Windows;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
			Session session = FindSession();
			var imageBytes = await document.LoadData(session);

			if (document.DocumentType == DocumentType.JPEG || document.DocumentType == DocumentType.PNG)
			{
				BitmapImage image = new BitmapImage();
				image.BeginInit();
				image.StreamSource = new MemoryStream(imageBytes);
				image.EndInit();

				ImageViewWindow imageViewWindow = new ImageViewWindow();
				imageViewWindow.DataContext = image;
				imageViewWindow.ShowDialog();
			}
			else if (document.DocumentType == DocumentType.PDF)
			{
				PdfToImageConverter converter = new PdfToImageConverter();
				await converter.ReadPdfFromBytes(imageBytes);

				PdfViewWindow pdfViewWindow = new PdfViewWindow();
				pdfViewWindow.DataContext = converter.Images.ToList();
				pdfViewWindow.ShowDialog();
			}
		}

		private async void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (!docIsOpen && DataContext is AttachmentMetaData atatchment)
				await ViewDocumentAttachment(atatchment);
		}

		private async void VewFileButton_Click(object sender, RoutedEventArgs e)
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
