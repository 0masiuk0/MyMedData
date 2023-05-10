using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Channels;
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
	/// Логика взаимодействия для EntityManager.xaml
	/// </summary>
	public partial class EntityManager : UserControl
	{
		public EntityManager()
		{
			InitializeComponent();
			_collectionViewSource = (CollectionViewSource)TryFindResource("EntitiesView");

			TitleTextBox.TextChanged += TextBox_TextChanged;
			CommentTextBox.TextChanged += TextBox_TextChanged;

			TitleTextBox.TextChanged += (o, e) => _collectionViewSource.View.Refresh();
			CommentTextBox.TextChanged += (o, e) => _collectionViewSource.View.Refresh();
        }
		
		public object? SelectedItem
		{
			get; private set;
		}

		CollectionViewSource _collectionViewSource;

		private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
            if (DataContext is ObservableCollection<ExaminationType> examinationTypesCollection)
			{
				_collectionViewSource.Source = examinationTypesCollection;
				TitleDataGridColumn.Binding = new Binding(nameof(ExaminationType.ExaminationTypeTitle));
				TitleDataGridColumn.Header = "Наименование";
			}
			else if (DataContext is ObservableCollection<Clinic> clinicTypesCollection)
			{
				_collectionViewSource.Source = clinicTypesCollection;
                TitleDataGridColumn.Binding = new Binding(nameof(Clinic.Name));
                TitleDataGridColumn.Header = "Наименование";
            }
			else if (DataContext is ObservableCollection<Doctor> doctorsCollection) 
			{
				_collectionViewSource.Source = doctorsCollection;
                TitleDataGridColumn.Binding = new Binding(nameof(Doctor.Name));
                TitleDataGridColumn.Header = "Имя";
            }
			else 
			{
				TitleDataGridColumn.Binding = null;
			}
        }
		        
		private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
		{
			if (e.Item is Doctor doc)
			{
				if (!string.IsNullOrWhiteSpace(TitleTextBox.Text))
				{
					if (!doc.Name.ToLower().StartsWith(TitleTextBox.Text))
					{
						e.Accepted = false;
						return;
					}
				}

				if(!string.IsNullOrWhiteSpace(CommentTextBox.Text))
				{
					if(!doc.Comment.ToLower().StartsWith(CommentTextBox.Text)) 
					{
						e.Accepted = false; 
						return; 
					}
				}

				e.Accepted = true;
			}
			else if (e.Item is ExaminationType examinationType) 
			{
                if (!string.IsNullOrWhiteSpace(TitleTextBox.Text))
                {
                    if (!examinationType.ExaminationTypeTitle.ToLower().StartsWith(TitleTextBox.Text))
                    {
                        e.Accepted = false;
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(CommentTextBox.Text))
                {
                    if (!examinationType.Comment.ToLower().StartsWith(CommentTextBox.Text))
                    {
                        e.Accepted = false;
                        return;
                    }
                }

                e.Accepted = true;
            }
			else if (e.Item is Clinic clinic)
			{
                if (!string.IsNullOrWhiteSpace(TitleTextBox.Text))
                {
                    if (!clinic.Name.ToLower().StartsWith(TitleTextBox.Text))
                    {
                        e.Accepted = false;
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(CommentTextBox.Text))
                {
                    if (!clinic.Comment.ToLower().StartsWith(CommentTextBox.Text))
                    {
                        e.Accepted = false;
                        return;
                    }
                }

				e.Accepted = true;
            }
			else 
				e.Accepted = true;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((DataContext is ObservableCollection<ExaminationType> examinationTypesCollection
				    && examinationTypesCollection.FirstOrDefault(t => t.ExaminationTypeTitle.ToLower() == TitleTextBox.Text.Trim(), null) is ExaminationType)
				|| (DataContext is ObservableCollection<Clinic> clinicTypesCollection
					&& clinicTypesCollection.FirstOrDefault(t => t.Name.ToLower() == TitleTextBox.Text.Trim(), null) is Clinic)
				|| (DataContext is ObservableCollection<Doctor> doctorsCollection
					&& doctorsCollection.FirstOrDefault(t => t.Name.ToLower() == TitleTextBox.Text.Trim(), null) is Doctor))
						AddNewEntityButton.IsEnabled = false;
                else
					    AddNewEntityButton.IsEnabled = !string.IsNullOrWhiteSpace(TitleTextBox.Text);            
        }

		private void AddNewEntityButton_Click(object sender, RoutedEventArgs e)
		{
			string title = TitleTextBox.Text;
            string comment = CommentTextBox.Text ?? "";
			switch(DataContext) 
			{
				case ObservableCollection<ExaminationType> cache: 
					var newItem = new ExaminationType(title, comment);
					cache.Add(newItem);
					EntitiesDataGrid.SelectedItem = newItem;
					break;
				case ObservableCollection<Doctor> cache: 
					var newItem2 = new Doctor(title, comment);
                    EntitiesDataGrid.SelectedItem = newItem2;
                    break;
				case ObservableCollection<Clinic>:
					var newItem3 = new Clinic(title, comment);
                    EntitiesDataGrid.SelectedItem = newItem3;
                    break;
				default: 
					SelectedItem = null; 
					break;
			}
        }

		private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			SelectedItem = EntitiesDataGrid.SelectedItem;
			RaiseEvent(new RoutedEventArgs(SelectionMadeEvent, this));
        }

        public static readonly RoutedEvent SelectionMadeEvent = EventManager.RegisterRoutedEvent(
            name: nameof(SelectionMade),
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(EntityManager));

        // Provide CLR accessors for adding and removing an event handler.
        public event RoutedEventHandler SelectionMade
        {
            add { AddHandler(SelectionMadeEvent, value); }
            remove { RemoveHandler(SelectionMadeEvent, value); }
        }
    }
}
