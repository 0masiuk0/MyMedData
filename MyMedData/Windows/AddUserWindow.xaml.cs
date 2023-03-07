using Microsoft.Win32;
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
using Ookii.Dialogs;
using System.IO;

namespace MyMedData.Windows
{
	/// <summary>
	/// Логика взаимодействия для AddUserWindow.xaml
	/// </summary>
	public partial class AddUserWindow : Window
	{
		public AddUserWindow()
		{
			InitializeComponent();
			UserNameTextBox.TextChanged += ValidateData;
			PasswordTextBox1.PasswordChanged += ValidateData;
			PasswordTextBox2.PasswordChanged += ValidateData;
			selectedColor = userPlaque.Foreground;
		}

		private void UserNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			userPlaque.Text = UserNameTextBox.Text;
		}

		Brush selectedColor;
		private void PickColorButton_Click(object sender, RoutedEventArgs e)
		{
			var colorPickerWindow = new ColorPickerWindow();
			colorPickerWindow.ShowDialog();
			userPlaque.Foreground = selectedColor = new SolidColorBrush(colorPickerWindow.SelectedColor);
		}

		string? dataFolder;
		private void EditDataFolderButton_Click(object sender, RoutedEventArgs e)
		{
			var openFolderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
			openFolderDialog.Multiselect = false;			
			if (openFolderDialog.ShowDialog() ?? false)
			{
				dataFolder = openFolderDialog.SelectedPath;
			}
			ValidateData();
		}

		public User? NewUser;

		private void OKbutton_Click(object sender, RoutedEventArgs e)
		{

		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{

		}

		private void ValidateData()
		{

		}

		private void ValidateData(object sender, RoutedEventArgs eventArgs)
		{
			ValidateData();
		}
	}
}
