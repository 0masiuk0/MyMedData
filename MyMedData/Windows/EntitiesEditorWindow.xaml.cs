﻿using ABI.Windows.ApplicationModel.Activation;
using MyMedData.Classes;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		Session CurrentSession;

		public EntitiesEditorWindow(Session session)
		{
			InitializeComponent();
			CurrentSession = session;
			entitiesCollectionViewSource = (CollectionViewSource)TryFindResource("EntitiesCollectionViewSource");

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
				SearchTextBox.Text = string.Empty;

				//checkbox switch operations
				CheckBox senderCheckBox = (CheckBox)sender;
				if (senderCheckBox.IsChecked == false) { senderCheckBox.IsChecked = true; return; }

				UnregisterCheckboxEventHandlers();
				ProcessCheckboxesStates(senderCheckBox);
				SetMode(senderCheckBox);

				//loading data
				LoadEntities();

				EntitiesListBox.Focus();
				if (Entities.Count > 0)
					EntitiesListBox.SelectedIndex = 0;
			}
			finally
			{
				RegisterCheckboxChangedHandlers();
			}
		}

		ObservableCollection<IdAndComment> _entities = new();
		public ObservableCollection<IdAndComment> Entities
		{
			get { return _entities; }
			set { _entities = value; }
		}


		private void LoadEntities()
		{
			Entities.Clear();
			IEnumerable<IdAndComment> entities;
			switch (Mode)
			{
				case EntityType.Doctor:
					entities = CurrentSession.DoctorCache.Select(d => new IdAndComment(d));
					break;
				case EntityType.DoctorType:
					entities = CurrentSession.DoctorTypesCache.Select(d => new IdAndComment(d));
					break;
				case EntityType.LabExaminaionType:
					entities = CurrentSession.LabTestTypesCache.Select(d => new IdAndComment(d));
					break;
				case EntityType.Clinic:
					entities = CurrentSession.ClinicCache.Select(d => new IdAndComment(d));
					break;
				default: throw new Exception("ImpossibleException 2");
			}
			foreach (var entity in entities) Entities.Add(entity);
		}

		IdAndComment? _editedEntity;
		IdAndComment? EditedEntity { get { return _editedEntity; } }
		private void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is not Grid manager)
				return;

			if (e.OldValue is IMedicalEntity oldValue && _editedEntity != null
				&& MessageBox.Show("Сохранить изменения?", "Несохраненные измения", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				ApplyChanges();
			}

			_editedEntity = null;

			if (e.NewValue is IdAndComment newValue)
			{
				_editedEntity = newValue.Copy();
				NameTextBox.Text = newValue.Id;
				CommentTextBox.Text = newValue.Comment;
			}
		}

		private void ApplyChanges()
		{
			
			_editedEntity = null;
		}	

		private void UpdateEditedEntity()
		{
			if (EditedEntity == null)
			{
				NameTextBox.Text = CommentTextBox.Text = string.Empty;
				return;
			}

			EditedEntity.Id = NameTextBox.Text ?? "";
			EditedEntity.Comment = CommentTextBox.Text ?? "";

			if (EntityViewGrid.DataContext is IdAndComment oldValue)
			{
				AcceptNameChangeButton.IsEnabled = !(EditedEntity.Id.Length == 0 || EditedEntity.Id == oldValue.Id);
				AcceptCommentChangeButton.IsEnabled = !(EditedEntity.Comment == oldValue.Comment 
					|| (string.IsNullOrWhiteSpace(EditedEntity.Comment) && string.IsNullOrWhiteSpace(oldValue.Comment)));
			}
			else
			{
				AcceptNameChangeButton.IsEnabled = AcceptCommentChangeButton.IsEnabled = false;		
			}	
		}

		private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
		{
			if (string.IsNullOrEmpty(SearchTextBox.Text))
			{
				e.Accepted = true;
				return;
			}

			if (e.Item is IdAndComment item && item.Id.ToLower().Contains(SearchTextBox.Text.ToLower()))
				e.Accepted = true;
			else
				e.Accepted = false;
		}

		//----------------------------------------------UI EVENTS---------------------------------------------------

		private void EntitiesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (EntitiesListBox.SelectedItem == null)
			{
				_editedEntity = null;
				UpdateEditedEntity();
			}
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
				
		private void MinimizeButton_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}

		private void EntitiesEditorWindow_Closed(object? sender, EventArgs e)
		{
			UnregisterCheckboxEventHandlers();
			CurrentSession = null;
		}
		private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateEditedEntity();
		}

		private void CommentTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateEditedEntity();
		}

		private void AcceptCommentChangeButton_Click(object sender, RoutedEventArgs e)
		{
			ApplyChanges();
		}

		private void AcceptNameChangeButton_Click(object sender, RoutedEventArgs e)
		{
			ApplyChanges();
		}

		private void NameTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && AcceptNameChangeButton.IsEnabled) ApplyChanges();
		}

		private void CommentTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && AcceptCommentChangeButton.IsEnabled) ApplyChanges();
		}

		private void DenyNameChangeButton_Click(object sender, RoutedEventArgs e)
		{
			if (EntityViewGrid.DataContext is IdAndComment oldValue)
				NameTextBox.Text = oldValue.Id;
		}

		private void DenyCommentChangeButton_Click(object sender, RoutedEventArgs e)
		{
			if (EntityViewGrid.DataContext is IdAndComment oldValue)
				CommentTextBox.Text = oldValue.Comment;
		}

		CollectionViewSource entitiesCollectionViewSource;
		private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			entitiesCollectionViewSource.View.Refresh();
			if (!entitiesCollectionViewSource.View.Cast<object>().Any())
				EntitiesListBox.SelectedIndex = -1;
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
				Mode = EntityType.Clinic;
			else if (senderCheckBox == DoctorTypeCheckBox)
				Mode = EntityType.DoctorType;
		}

	}
}
