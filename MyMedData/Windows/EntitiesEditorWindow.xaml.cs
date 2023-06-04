using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

namespace MyMedData.Windows
{
    /// <summary>
    /// Interaction logic for EntitiesEditorWindow.xaml
    /// </summary>
    public partial class EntitiesEditorWindow : Window
    {
		readonly DependencyPropertyDescriptor propertyDecriptor;

        public EntitiesEditorWindow()
        {
            InitializeComponent();
			Closed += EntitiesEditorWindow_Closed;
			propertyDecriptor = DependencyPropertyDescriptor.FromProperty(CheckBox.IsCheckedProperty, typeof(CheckBox));
			checkBoxes = new CheckBox[] { DoctorCheckBox, LabExamTypeCheckBox, ClinicCheckBox, DoctorTypeCheckBox };
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			RegisterCheckboxChangedHandlers();

			DoctorTypeCheckBox.IsChecked = true;
		}

		EntityType Mode = EntityType.Doctor;

		CheckBox[] checkBoxes; 
		private void EntityTypeSelectionChanged(object sender, EventArgs e)
		{
			try
			{
				CheckBox senderCheckBox = (CheckBox)sender;
				if (senderCheckBox.IsChecked == false)
				{
					senderCheckBox.IsChecked = true;
					return;
				}

				UnregisterCheckboxEventHandlers();
				ProcessCheckboxesStates(senderCheckBox);
				SetMode(senderCheckBox);
			}
			finally
			{
				RegisterCheckboxChangedHandlers();
			}
		}		

		private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{

		}

		private void EntitiesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
            Close();
        }

        Thickness _normalPadding = new Thickness(0);
        Thickness _maximizedStatePadding = new Thickness(8);
		private void RestoreButton_Click(object sender, RoutedEventArgs e)
		{
            WindowState = WindowState.Normal;
            MaximizeButton.Visibility = Visibility.Visible; 
            RestoreButton.Visibility = Visibility.Collapsed;
            Padding = _normalPadding;
        }

		private void MaximizeButton_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Maximized;
			RestoreButton.Visibility = Visibility.Visible;
			MaximizeButton.Visibility = Visibility.Collapsed;
            Padding = _maximizedStatePadding;
		}

		private void MinimizeButton_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}

		private void EntitiesEditorWindow_Closed(object? sender, EventArgs e)
		{
			UnregisterCheckboxEventHandlers();
		}		

		//-------------------------------AUXILIARIES----------------------------------------------
		private void RegisterCheckboxChangedHandlers()
		{
			propertyDecriptor.AddValueChanged(DoctorTypeCheckBox, EntityTypeSelectionChanged);
			propertyDecriptor.AddValueChanged(LabExamTypeCheckBox, EntityTypeSelectionChanged);
			propertyDecriptor.AddValueChanged(ClinicCheckBox, EntityTypeSelectionChanged);
			propertyDecriptor.AddValueChanged(DoctorCheckBox, EntityTypeSelectionChanged);
		}

		private void UnregisterCheckboxEventHandlers()
		{
			propertyDecriptor.RemoveValueChanged(DoctorTypeCheckBox, EntityTypeSelectionChanged);
			propertyDecriptor.RemoveValueChanged(DoctorCheckBox, EntityTypeSelectionChanged);
			propertyDecriptor.RemoveValueChanged(LabExamTypeCheckBox, EntityTypeSelectionChanged);
			propertyDecriptor.RemoveValueChanged(ClinicCheckBox, EntityTypeSelectionChanged);
		}

		private void ProcessCheckboxesStates(CheckBox checkboxOn)
		{
			foreach (var checkBox in checkBoxes)
			{
				if (!object.ReferenceEquals(checkBox, checkboxOn))
					checkBox.IsChecked = false;
			}
		}

		private void SetMode(CheckBox senderCheckBox)
		{
			if (senderCheckBox == DoctorCheckBox)
				Mode = EntityType.Doctor;
			else if (senderCheckBox == LabExamTypeCheckBox)
				Mode = EntityType.LabExaminaionType;
			else if (senderCheckBox == ClinicCheckBox)
				Mode = EntityType.CLinic;
			else if (senderCheckBox == DoctorTypeCheckBox)
				Mode = EntityType.DoctorType;
		}

		enum EntityType
		{
			Doctor,
			DoctorType,
			LabExaminaionType,
			CLinic
		}
	}
}
