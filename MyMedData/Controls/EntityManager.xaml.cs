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
using Windows.Devices.HumanInterfaceDevice;

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

			UpdateNewEntityButtonEnabling();
        }
		        
		private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
		{
			if (e.Item is Doctor doc)
			{
				if (!string.IsNullOrWhiteSpace(TitleTextBox.Text.ToLower()))
				{
					if (!doc.Name.ToLower().StartsWith(TitleTextBox.Text.ToLower()))
					{
						e.Accepted = false;
						return;
					}
				}

				if(!string.IsNullOrWhiteSpace(CommentTextBox.Text))
				{
					if(!doc.Comment?.ToLower().StartsWith(CommentTextBox.Text.ToLower()) ?? true) 
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
                    if (!examinationType.ExaminationTypeTitle?.ToLower().StartsWith(TitleTextBox.Text.ToLower()) ?? true)
                    {
                        e.Accepted = false;
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(CommentTextBox.Text))
                {
                    if (!examinationType.Comment?.ToLower().StartsWith(CommentTextBox.Text.ToLower()) ?? true)
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
                    if (!clinic.Name.ToLower().StartsWith(TitleTextBox.Text.ToLower()))
                    {
                        e.Accepted = false;
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(CommentTextBox.Text))
                {
                    if (!clinic.Comment?.ToLower().StartsWith(CommentTextBox.Text.ToLower()) ?? true)
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

		bool _entityAdditionEnabled;
		bool entityAdditionEnabled
		{
			get { return _entityAdditionEnabled; }
			set 
			{
				_entityAdditionEnabled = value;
				AddNewEntityButton.IsEnabled = value;
				AddNewEntityButton.Visibility = _entityAdditionEnabled ? Visibility.Visible : Visibility.Collapsed;
			}
		}


		private void UpdateNewEntityButtonEnabling()
		{
			if (
				//this is exmainationType dialog AND such type exists in cache
				(DataContext is ObservableCollection<ExaminationType> examinationTypesCollection 
							&& examinationTypesCollection.FirstOrDefault(t => t.ExaminationTypeTitle.ToLower() == TitleTextBox.Text.ToLower().Trim(), null) is ExaminationType)
				//OR this is clinic dialog AND such clinic exists in cache
				|| (DataContext is ObservableCollection<Clinic> clinicCollection
							&& clinicCollection.FirstOrDefault(t => t.Name.ToLower() == TitleTextBox.Text.ToLower().Trim(), null) is Clinic)
				//OR this is Doctor dialog AND such doctor exists in cache
				|| (DataContext is ObservableCollection<Doctor> doctorsCollection
							&& doctorsCollection.FirstOrDefault(d => d.Name.ToLower() == TitleTextBox.Text.ToLower().Trim(), null) is Doctor))
			{
				entityAdditionEnabled = false;
				AddNewEntityButton.Visibility = Visibility.Collapsed;
			}
			else
			{
				entityAdditionEnabled = !string.IsNullOrWhiteSpace(TitleTextBox.Text);
			}
		}

		private void AddNewEntityButton_Click(object sender, RoutedEventArgs e)
		{
			AddNewEntity();
		}

		private void AddNewEntity()
		{
			if (!entityAdditionEnabled)
				return;

			string title = TitleTextBox.Text;
			string comment = CommentTextBox.Text ?? "";
			switch (DataContext)
			{
				case ObservableCollection<ExaminationType> cache:
					var newItem = new ExaminationType(title, comment);
					cache.Add(newItem);
					EntitiesDataGrid.SelectedItem = newItem;
					break;
				case ObservableCollection<Doctor> cache:
					var newItem2 = new Doctor(title, comment);
					cache.Add(newItem2);
					EntitiesDataGrid.SelectedItem = newItem2;
					break;
				case ObservableCollection<Clinic> cache:
					var newItem3 = new Clinic(title, comment);
					cache.Add(newItem3);
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
			RaiseEvent(new RoutedEventArgs(SelectionDoneEvent, this));
        }

        public static readonly RoutedEvent SelectionDoneEvent = EventManager.RegisterRoutedEvent(
            name: nameof(SelectionDone),
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(EntityManager));

        // Provide CLR accessors for adding and removing an event handler.
        public event RoutedEventHandler SelectionDone
        {
            add { AddHandler(SelectionDoneEvent, value); }
            remove { RemoveHandler(SelectionDoneEvent, value); }
        }

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateNewEntityButtonEnabling();
		}

		internal void Clear()
		{
			TitleTextBox.Text = string.Empty;
			CommentTextBox.Text = string.Empty;
			SelectedItem = null;
		}

		private void EntitiesDataGrid_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (EntitiesDataGrid.SelectedItem != null)
				{
					SelectedItem = EntitiesDataGrid.SelectedItem;
					RaiseEvent(new RoutedEventArgs(SelectionDoneEvent, this));
				}
				else if (entityAdditionEnabled)
					AddNewEntity();
			}
			else if (e.Key == Key.Escape)
				Cancel();			
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			UpdateNewEntityButtonEnabling();
			EntitiesDataGrid.Focus();
		}

		private void UserControl_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Escape)
				Cancel();
		}

		private void Cancel()
		{
			SelectedItem = null;
			RaiseEvent(new RoutedEventArgs(SelectionDoneEvent, this));
		}
	}
}
